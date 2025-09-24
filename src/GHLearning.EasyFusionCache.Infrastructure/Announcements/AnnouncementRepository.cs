using GHLearning.EasyFusionCache.Domain.Announcements;
using GHLearning.EasyFusionCache.Infrastructure.Announcements.Tables;
using MongoDB.Driver;

namespace GHLearning.EasyFusionCache.Infrastructure.Announcements;
internal class AnnouncementRepository : IAnnouncementRepository
{
    private readonly IMongoCollection<AnnouncementsTable> _collection;
    public AnnouncementRepository(
        IMongoCollection<AnnouncementsTable> collection)
    {
        _collection = collection;

        // Ensure Indexes (title text, createdAt, status, expiresAt)
        var indexKeys = Builders<AnnouncementsTable>.IndexKeys
            .Text(a => a.Title)
            .Ascending(a => a.CreatedAt)
            .Ascending(a => a.Status)
            .Ascending(a => a.ExpiresAt);

        var idxModel = new CreateIndexModel<AnnouncementsTable>(indexKeys);
        _collection.Indexes.CreateOne(idxModel);
    }

    public Task AddAsync(Announcement announcement, CancellationToken cancellationToken = default)
    {
        var table = MapToTable(announcement);
        return _collection.InsertOneAsync(table, null, cancellationToken);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<AnnouncementsTable>.Filter.Eq(a => a.Id, id);
        return _collection.DeleteOneAsync(filter, cancellationToken);
    }

    public async IAsyncEnumerable<Announcement> GetAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 過濾 Status 並排除軟刪除
        var filter = Builders<AnnouncementsTable>.Filter.Eq(a => a.IsDeleted, false);

        using var cursor = await _collection.FindAsync(filter, cancellationToken: cancellationToken).ConfigureAwait(false);
        while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
        {
            foreach (var table in cursor.Current)
            {
                yield return MapToDomain(table);
            }
        }
    }

    public async Task<Announcement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<AnnouncementsTable>.Filter.Eq(a => a.Id, id) &
                     Builders<AnnouncementsTable>.Filter.Eq(a => a.IsDeleted, false);

        var table = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return table == null ? null : MapToDomain(table);
    }

    public Task SoftDeleteAsync(Announcement announcement, CancellationToken cancellationToken = default)
    {
        var filter = Builders<AnnouncementsTable>.Filter.Eq(a => a.Id, announcement.Id);
        var update = Builders<AnnouncementsTable>.Update.Set(a => a.IsDeleted, true)
            .Set(a => a.Status, AnnouncementStatus.SoftDelete);
        return _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public Task UpdateAsync(Announcement announcement, CancellationToken cancellationToken = default)
    {
        var filter = Builders<AnnouncementsTable>.Filter.Eq(a => a.Id, announcement.Id);
        var table = MapToTable(announcement);
        return _collection.ReplaceOneAsync(filter, table, new ReplaceOptions { IsUpsert = false }, cancellationToken);
    }

    // 將 Domain Entity 轉成 Table
    private static AnnouncementsTable MapToTable(Announcement announcement)
    {
        return new AnnouncementsTable
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            CreatedBy = announcement.CreatedBy,
            CreatedAt = announcement.CreatedAt.UtcDateTime,
            PublishedAt = announcement.PublishedAt?.UtcDateTime,
            ExpiresAt = announcement.ExpiresAt?.UtcDateTime,
            Status = announcement.Status,
            IsPinned = announcement.IsPinned,
            IsDeleted = announcement.IsDeleted
        };
    }

    // 將 Table 轉回 Domain Entity
    private static Announcement MapToDomain(AnnouncementsTable table)
    {
        var announcement = new Announcement
        {
            Id = table.Id,
            Title = table.Title,
            Content = table.Content,
            CreatedAt = table.CreatedAt,
            CreatedBy = table.CreatedBy,
            PublishedAt = table.PublishedAt,
            ExpiresAt = table.ExpiresAt,
            Status = table.Status,
            IsPinned = table.IsPinned
        };

        if (table.IsDeleted)
            announcement.SoftDelete();

        return announcement;
    }
}
