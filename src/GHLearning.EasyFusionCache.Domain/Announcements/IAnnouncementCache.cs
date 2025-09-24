namespace GHLearning.EasyFusionCache.Domain.Announcements;
public interface IAnnouncementCache
{
	/// <summary>
	/// 依狀態取得公告快取，若沒有快取則由實作決定行為
	/// </summary>
	/// <param name="status">公告狀態</param>
	/// <param name="cancellationToken">CancellationToken</param>
	/// <returns>公告集合</returns>
	IAsyncEnumerable<Announcement> GetAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// 將指定狀態公告寫入快取
	/// </summary>
	/// <param name="status">公告狀態</param>
	/// <param name="cancellationToken">CancellationToken</param>
	Task SetAsync(CancellationToken cancellationToken = default);
}
