using MediatR;

namespace GHLearning.EasyFusionCache.Application.Announcements.Create;
public record CreateAnnouncementCommand(string Title, string Content, string CreatedBy)
	: IRequest<Guid>;
