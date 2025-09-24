using GHLearning.EasyFusionCache.Domain.Announcements;
using MediatR;

namespace GHLearning.EasyFusionCache.Application.Announcements.Get;
internal class GetAnnouncementByIdHandler(
	IAnnouncementCache cache) : IRequestHandler<GetAnnouncementByIdQuery, AnnouncementDto?>
{
	public async Task<AnnouncementDto?> Handle(GetAnnouncementByIdQuery request, CancellationToken cancellationToken)
	{
		var allAnnouncements = await cache.GetAsync(cancellationToken).ToArrayAsync(cancellationToken).ConfigureAwait(false);

		var announcement = allAnnouncements.FirstOrDefault(a => a.Id == request.Id);

		return announcement == null
			? null
			: new AnnouncementDto
			{
				Id = announcement.Id,
				Title = announcement.Title,
				Content = announcement.Content,
				CreatedBy = announcement.CreatedBy,
				CreatedAt = announcement.CreatedAt,
				PublishedAt = announcement.PublishedAt,
				ExpiresAt = announcement.ExpiresAt,
				Status = announcement.Status.ToString(),
				IsPinned = announcement.IsPinned
			};
	}
}