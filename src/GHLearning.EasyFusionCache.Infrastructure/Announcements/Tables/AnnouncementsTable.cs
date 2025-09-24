using GHLearning.EasyFusionCache.Domain.Announcements;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GHLearning.EasyFusionCache.Infrastructure.Announcements.Tables;
public record AnnouncementsTable
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    /// <summary>
    /// 公告唯一識別 ID，MongoDB ObjectId 生成，存為 string。
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// 公告標題，必填欄位，用於列表顯示和搜尋。
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// 公告內容，必填欄位，存放公告詳細說明。
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// 建立者帳號或名稱，用於追蹤誰創建了公告。
    /// </summary>
    public required string CreatedBy { get; set; }
    /// <summary>
    /// 公告建立時間，UTC 時間。
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 公告發佈時間，只有已發布公告才有值。
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// 公告過期時間，超過此時間公告可視為失效或不顯示。
    /// 可為 null，表示永不過期。
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 公告狀態：Draft（草稿）、Published（已發布）、Archived（已封存）。
    /// 控制公告行為，例如發布後不可直接更新。
    /// </summary>
    public AnnouncementStatus Status { get; set; }

    /// <summary>
    /// 是否置頂，用於列表排序顯示優先。
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// 軟刪除標誌，為 true 表示公告已被刪除，但資料仍保留於資料庫。
    /// 用於安全刪除或恢復功能。
    /// </summary>
    public bool IsDeleted { get; set; }
}
