namespace GHLearning.EasyFusionCache.Domain.Announcements;

/// <summary>
/// 公告的生命週期狀態
/// </summary>
public enum AnnouncementStatus
{
	/// <summary>
	/// 草稿狀態：尚未公開，僅建立者或具備權限者可見。
	/// 可修改標題與內容。
	/// </summary>
	Draft,

	/// <summary>
	/// 已發佈狀態：對所有使用者公開顯示。
	/// 不允許直接修改內容，如需修改需先下架或建立新版本。
	/// </summary>
	Published,

	/// <summary>
	/// 已封存狀態：不再對外顯示，但仍保留於系統中做為紀錄。
	/// 通常由已發佈狀態轉換而來。
	/// </summary>
	Archived,

	/// <summary>
	/// 軟刪除狀態：從系統中移除，不再顯示於任何使用者。
	/// </summary>
	SoftDelete
}
