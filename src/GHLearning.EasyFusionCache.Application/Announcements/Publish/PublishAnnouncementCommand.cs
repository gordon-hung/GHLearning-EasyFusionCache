using MediatR;

namespace GHLearning.EasyFusionCache.Application.Announcements.Publish;
public record PublishAnnouncementCommand(
	Guid Id,
	DateTime PublishedAt,
	DateTime? ExpiresAt = null) : IRequest;