using GHLearning.EasyFusionCache.Domain.Announcements;
using MediatR;

namespace GHLearning.EasyFusionCache.Application.Announcements.Query;
public record QueryAnnouncementsQuery(AnnouncementStatus Status, int Page = 1, int PageSize = 20, string? Search = null)
	: IRequest<(IEnumerable<AnnouncementDto> Items, long Total)>;
