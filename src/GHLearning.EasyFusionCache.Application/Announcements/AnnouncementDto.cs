namespace GHLearning.EasyFusionCache.Application.Announcements;

public class AnnouncementDto
{
	public required Guid Id { get; set; }
	public required string Title { get; set; }
	public required string Content { get; set; }
	public required string CreatedBy { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? PublishedAt { get; set; }
	public DateTimeOffset? ExpiresAt { get; set; }
	public required string Status { get; set; }
	public bool IsPinned { get; set; }
}
