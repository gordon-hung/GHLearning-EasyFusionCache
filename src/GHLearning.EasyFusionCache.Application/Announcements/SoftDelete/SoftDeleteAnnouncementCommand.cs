using MediatR;

namespace GHLearning.EasyFusionCache.Application.Announcements.SoftDelete;
public record SoftDeleteAnnouncementCommand(Guid Id) : IRequest;
