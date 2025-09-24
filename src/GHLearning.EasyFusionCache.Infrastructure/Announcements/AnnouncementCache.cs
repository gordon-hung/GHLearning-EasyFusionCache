using System.Runtime.CompilerServices;
using GHLearning.EasyFusionCache.Domain.Announcements;
using ZiggyCreatures.Caching.Fusion;

namespace GHLearning.EasyFusionCache.Infrastructure.Announcements;
internal class AnnouncementCache(
    IFusionCache fusionCache,
    IAnnouncementRepository repo) : IAnnouncementCache
{
	private readonly string _key = nameof(AnnouncementCache);

    private readonly FusionCacheEntryOptions _fusionCacheEntryOptions = new()
    {
        // -------------------------
        // 本地快取設定（Local Cache）
        // -------------------------
        Duration = TimeSpan.FromMinutes(5),             // 本地快取有效時間
        IsFailSafeEnabled = true,                       // 啟用本地 FailSafe（刷新失敗仍返回舊值）
        FailSafeMaxDuration = TimeSpan.FromMinutes(10), // 本地 FailSafe
        FactorySoftTimeout = TimeSpan.FromSeconds(3),   // 快取工廠軟超時（超過此時間可能返回舊值）
        FactoryHardTimeout = TimeSpan.FromSeconds(5),   // 快取工廠硬超時（超過此時間視為刷新失敗）

        // -------------------------
        // 分散式快取設定（Distributed Cache，例如 Redis / Memcached）
        // -------------------------
        DistributedCacheDuration = TimeSpan.FromMinutes(30),            // 分散式快取有效時間
        DistributedCacheSoftTimeout = TimeSpan.FromSeconds(5),          // 分散式快取軟超時
        DistributedCacheHardTimeout = TimeSpan.FromSeconds(10),         // 分散式快取硬超時
        DistributedCacheFailSafeMaxDuration = TimeSpan.FromMinutes(60), // 分散式 FailSafe

        // -------------------------
        // 多節點分散式應用設定（協調多節點快取行為）
        // -------------------------
        SkipDistributedCacheRead = false,        // 是否跳過讀取分散式快取（false 表示正常讀取）
        SkipDistributedCacheWrite = false,       // 是否跳過寫入分散式快取（false 表示正常寫入）
        SkipBackplaneNotifications = false,      // 是否跳過跨節點快取更新通知（false 表示會通知其他節點）

        // -------------------------
        // 積極刷新設定（Eager Refresh）
        // -------------------------
        EagerRefreshThreshold = 0.7f,

        // -------------------------
        // 高可用與隨機抖動（Jitter）設定
        // -------------------------
        JitterMaxDuration = TimeSpan.FromSeconds(5),

        // -------------------------
        // 背景處理設定
        // -------------------------
        AllowBackgroundDistributedCacheOperations = true
    };

    public async IAsyncEnumerable<Announcement> GetAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var announcements = await fusionCache.GetOrSetAsync<Announcement[]>(
            key: _key,
            factory: async (option, cancellationToken) =>
            await repo.GetAllAsync(cancellationToken: cancellationToken)
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false),
            options: _fusionCacheEntryOptions,
            token: cancellationToken).ConfigureAwait(false);

        foreach (var announcement in announcements)
        {
            yield return announcement;
        }
    }
    public async Task SetAsync(CancellationToken cancellationToken = default)
    {
        var announcements = await repo.GetAllAsync(
            cancellationToken: cancellationToken)
            .ToArrayAsync(
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await fusionCache.SetAsync(
            key: _key,
            announcements,
            options: _fusionCacheEntryOptions,
            token: cancellationToken)
            .ConfigureAwait(false);
    }
}
