namespace GHLearning.EasyFusionCache.Domain.Announcements;
public interface IAnnouncementRepository
{
	Task<Announcement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task AddAsync(Announcement announcement, CancellationToken cancellationToken = default);
	Task UpdateAsync(Announcement announcement, CancellationToken cancellationToken = default);
	Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
	Task SoftDeleteAsync(Announcement announcement, CancellationToken cancellationToken = default);
	IAsyncEnumerable<Announcement> GetAllAsync(CancellationToken cancellationToken = default);
}
