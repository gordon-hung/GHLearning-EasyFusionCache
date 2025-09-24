using System.Reflection;
using GHLearning.EasyFusionCache.Application.Abstractions.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace GHLearning.EasyFusionCache.Application.DependencyInjection;
public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddMediatR(cfg =>
		{
			cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
			cfg.AddOpenBehavior(typeof(HandleTracingPipelineBehavior<,>));
			cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
		});
		return services;
	}
}