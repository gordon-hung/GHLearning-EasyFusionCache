using MediatR;

namespace GHLearning.EasyFusionCache.Application.Announcements.Archive;
public record ArchiveAnnouncementCommand(Guid Id) : IRequest;