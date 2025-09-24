using GHLearning.EasyFusionCache.Domain.Announcements;
using MediatR;

namespace GHLearning.EasyFusionCache.Application.Announcements.Query;
internal class GetAnnouncementsHandler(
	IAnnouncementCache cache) : IRequestHandler<QueryAnnouncementsQuery, (IEnumerable<AnnouncementDto> Items, long Total)>
{
	public async Task<(IEnumerable<AnnouncementDto> Items, long Total)> Handle(
		QueryAnnouncementsQuery request,
		CancellationToken cancellationToken)
	{
		var allAnnouncements = await cache.GetAsync(cancellationToken).ToArrayAsync(cancellationToken).ConfigureAwait(false);

		var filtered = allAnnouncements
			.Where(a => a.Status == request.Status);

		if (!string.IsNullOrEmpty(request.Search))
			filtered = filtered.Where(a => a.Title.Contains(request.Search) || a.Content.Contains(request.Search));

		var total = filtered.Count();

		var paged = filtered
			.OrderByDescending(a => a.IsPinned)
			.ThenByDescending(a => a.PublishedAt ?? a.CreatedAt)
			.Skip((request.Page - 1) * request.PageSize)
			.Take(request.PageSize)
			.Select(a => new AnnouncementDto
			{
				Id = a.Id,
				Title = a.Title,
				Content = a.Content,
				CreatedBy = a.CreatedBy,
				CreatedAt = a.CreatedAt,
				PublishedAt = a.PublishedAt,
				ExpiresAt = a.ExpiresAt,
				Status = a.Status.ToString(),
				IsPinned = a.IsPinned
			});

		return (paged, total);
	}
}