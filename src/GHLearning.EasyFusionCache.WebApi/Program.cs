using System.Net.Mime;
using System.Text.Json.Serialization;
using CorrelationId;
using GHLearning.EasyFusionCache.Application.Announcements.CacheEvent;
using GHLearning.EasyFusionCache.Application.DependencyInjection;
using GHLearning.EasyFusionCache.Infrastructure;
using GHLearning.EasyFusionCache.Infrastructure.DependencyInjection;
using GHLearning.EasyFusionCache.SharedKernel;
using GHLearning.EasyFusionCache.WebApi.Middlewares;
using MassTransit;
using MassTransit.Logging;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
	.AddRouting(options => options.LowercaseUrls = true)
	.AddControllers(options =>
	{
		options.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
		options.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
	})
	.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
	.AddOptions<MongoOptions>()
	.Bind(builder.Configuration.GetSection(nameof(MongoOptions)))
	.Validate(options =>
		!string.IsNullOrWhiteSpace(options.ConnectionString) &&
		!string.IsNullOrWhiteSpace(options.Database) &&
		!string.IsNullOrWhiteSpace(options.AnnouncementCollection),
		"MongoOptions must have ConnectionString, Database and AnnouncementCollection configured"
	)
	.ValidateOnStart()
	.Services
	.AddInfrastructure()
	.AddApplication();

builder.Services
	.AddStackExchangeRedisCache(options =>
	{
		options.InstanceName = string.Concat(builder.Environment.ApplicationName, ":", builder.Environment.EnvironmentName, ':');
		options.ConfigurationOptions = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection string is not configured."));
	})
	.AddFusionCache()
	.WithDefaultEntryOptions(new FusionCacheEntryOptions
	{
		Duration = TimeSpan.FromMinutes(1),
	})
	// 使用 Message Pack 作為序列化工具
	.WithSystemTextJsonSerializer()
	// 使用 Redis 作為分散式快取的服務
	.WithDistributedCache(sp => sp.GetRequiredService<IDistributedCache>())
	.WithBackplane(new RedisBackplane(new RedisBackplaneOptions
	{
		ConfigurationOptions = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection string is not configured."))
	}));

builder.Services.AddMassTransit(x =>
{
	x.AddConsumer<CacheAnnouncementConsumer>();

	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

		cfg.ReceiveEndpoint("announcement-cache-updated", e =>
		{
			e.ConfigureConsumer<CacheAnnouncementConsumer>(context);
			e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10)));
			e.DiscardFaultedMessages();
		});
	});
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Learn more about configuring HttpLogging at https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/?view=aspnetcore-8.0
builder.Services.AddHttpLogging(logging =>
{
	logging.LoggingFields = HttpLoggingFields.All;
	logging.RequestHeaders.Add(CorrelationIdOptions.DefaultHeader);
	logging.ResponseHeaders.Add(CorrelationIdOptions.DefaultHeader);
	logging.RequestHeaders.Add(TraceHeaders.TraceParent);
	logging.ResponseHeaders.Add(TraceHeaders.TraceParent);
	logging.RequestHeaders.Add(TraceHeaders.TraceId);
	logging.ResponseHeaders.Add(TraceHeaders.TraceId);
	logging.RequestHeaders.Add(TraceHeaders.ParentId);
	logging.ResponseHeaders.Add(TraceHeaders.ParentId);
	logging.RequestHeaders.Add(TraceHeaders.TraceFlag);
	logging.ResponseHeaders.Add(TraceHeaders.TraceFlag);
	logging.RequestBodyLogLimit = 4096;
	logging.ResponseBodyLogLimit = 4096;
	logging.CombineLogs = true;
});

//Learn more about configuring OpenTelemetry at https://learn.microsoft.com/zh-tw/dotnet/core/diagnostics/observability-with-otel
builder.Services.AddOpenTelemetry()
	.ConfigureResource(resource => resource
	.AddService(
		serviceName: builder.Configuration["ServiceName"]!.ToLower(),
		serviceNamespace: builder.Configuration["ServiceNamespace"]!,
		serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown")
	)
	.UseOtlpExporter(OtlpExportProtocol.Grpc, new Uri(builder.Configuration["OtlpEndpointUrl"]!))
	.WithMetrics(metrics => metrics
		.AddMeter("GHLearning.")
		.AddAspNetCoreInstrumentation()
		.AddRuntimeInstrumentation()
		.AddProcessInstrumentation()
		.AddPrometheusExporter())
	.WithTracing(tracing => tracing
		.AddSource("GHLearning.")
		.AddSource(DiagnosticHeaders.DefaultListenerName)
		.AddHttpClientInstrumentation()
		.AddAspNetCoreInstrumentation(options => options.Filter = (httpContext) =>
				!httpContext.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/live", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/healthz", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/favicon.ico", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.Value!.Equals("/api/events/raw", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.Value!.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/_vs", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase)));

//Learn more about configuring HealthChecks at https://learn.microsoft.com/zh-tw/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-9.0
builder.Services.AddHealthChecks()
	.AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
	.AddRedis(builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection string is not configured."), "Redis")
	.AddMongoDb(clientFactory: sp => sp.GetRequiredService<IMongoClient>(), name: "MongoDb");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1"));// swagger/
app.UseReDoc(options => options.SpecUrl("/openapi/v1.json"));//api-docs/
app.MapScalarApiReference();//scalar/v1

app.UseHttpsRedirection();

app.UseCorrelationId();

app.UseMiddleware<TraceMiddleware>();

app.UseMiddleware<CorrelationMiddleware>();

app.UseHttpLogging();

app.UseAuthorization();

app.MapControllers();

app.UseHealthChecks("/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
	Predicate = check => check.Tags.Contains("live"),
	ResultStatusCodes =
	{
		[HealthStatus.Healthy] = StatusCodes.Status200OK,
		[HealthStatus.Degraded] = StatusCodes.Status200OK,
		[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
	}
});
app.UseHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
	Predicate = _ => true,
	ResultStatusCodes =
	{
		[HealthStatus.Healthy] = StatusCodes.Status200OK,
		[HealthStatus.Degraded] = StatusCodes.Status200OK,
		[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
	}
});

app.Run();
