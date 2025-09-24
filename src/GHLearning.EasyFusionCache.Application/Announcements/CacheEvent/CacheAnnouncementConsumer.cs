using GHLearning.EasyFusionCache.Domain.Announcements;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace GHLearning.EasyFusionCache.Application.Announcements.CacheEvent;
internal class CacheAnnouncementConsumer(
	ILogger<CacheAnnouncementConsumer> Logger,
	IAnnouncementCache Cache) : IConsumer<AnnouncementCacheDomainEvent>
{
	public Task Consume(ConsumeContext<AnnouncementCacheDomainEvent> context)
	{
		Logger.LogInformation("Received AnnouncementCacheDomainEvent: {Id}, {Status}", context.Message.Id, context.Message.Status);

		return Cache.SetAsync(context.CancellationToken);
	}
}

