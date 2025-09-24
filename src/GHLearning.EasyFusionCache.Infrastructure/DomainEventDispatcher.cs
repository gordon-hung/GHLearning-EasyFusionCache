using GHLearning.EasyFusionCache.Domain.Announcements;
using GHLearning.EasyFusionCache.SharedKernel;
using MassTransit;

namespace GHLearning.EasyFusionCache.Infrastructure;
internal class DomainEventDispatcher(
    IPublishEndpoint publish) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            switch (domainEvent)
            {
                case AnnouncementCacheDomainEvent e:
                    await publish.Publish(new AnnouncementCacheDomainEvent(
						Id: e.Id,
						Status: e.Status),
                        cancellationToken)
                        .ConfigureAwait(false);
                    break;
            }
        }
    }
}
