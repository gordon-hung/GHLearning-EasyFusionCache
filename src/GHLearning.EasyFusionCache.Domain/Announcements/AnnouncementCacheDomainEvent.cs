using GHLearning.EasyFusionCache.SharedKernel;

namespace GHLearning.EasyFusionCache.Domain.Announcements;
public record AnnouncementCacheDomainEvent(Guid Id, AnnouncementStatus Status) : IDomainEvent
{
	public DateTimeOffset OccurredOn => DateTimeOffset.UtcNow;
}
