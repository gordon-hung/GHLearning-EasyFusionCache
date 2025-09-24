using MediatR;

namespace GHLearning.EasyFusionCache.Application.Announcements.Get;
public record GetAnnouncementByIdQuery(Guid Id) : IRequest<AnnouncementDto?>;
