using CorrelationId;
using CorrelationId.DependencyInjection;
using GHLearning.EasyFusionCache.Domain.Announcements;
using GHLearning.EasyFusionCache.Infrastructure.Announcements;
using GHLearning.EasyFusionCache.Infrastructure.Announcements.Tables;
using GHLearning.EasyFusionCache.SharedKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GHLearning.EasyFusionCache.Infrastructure.DependencyInjection;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        => services
        .AddCorrelationInfrastructure()
        .AddAnnouncementRepository();

    private static IServiceCollection AddAnnouncementRepository(
        this IServiceCollection services)
        => services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
            return new MongoClient(options.ConnectionString);
        })
        .AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(options.Database);
            return database.GetCollection<AnnouncementsTable>(options.AnnouncementCollection);
        })
        .AddTransient<IAnnouncementRepository, AnnouncementRepository>()
        .AddTransient<IAnnouncementCache, AnnouncementCache>()
        .AddTransient<IDomainEventDispatcher, DomainEventDispatcher>();

    private static IServiceCollection AddCorrelationInfrastructure(this IServiceCollection services)
        => services.AddCorrelationId<CustomCorrelationIdProvider>(options =>
        {
            //Learn more about configuring CorrelationId at https://github.com/stevejgordon/CorrelationId/wiki
            options.AddToLoggingScope = true;
            options.LoggingScopeKey = CorrelationIdOptions.DefaultHeader;
        })
        .Services;
}
