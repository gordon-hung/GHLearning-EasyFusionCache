using GHLearning.EasyFusionCache.SharedKernel;

namespace GHLearning.EasyFusionCache.Domain.Announcements;

public class Announcement
{

	private readonly List<IDomainEvent> _domainEvents = [];

	public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

	public Guid Id { get; set; }

	public string Title { get; set; }

	public string Content { get; set; }

	public string CreatedBy { get; set; }

	public DateTimeOffset CreatedAt { get; set; }

	public DateTimeOffset? PublishedAt { get; set; }

	public DateTimeOffset? ExpiresAt { get; set; }

	public AnnouncementStatus Status { get; set; }

	public bool IsPinned { get; set; } = false;

	public bool IsDeleted { get; set; } = false;

	public Announcement()
	{
		Title = string.Empty;
		Content = string.Empty;
		CreatedBy = string.Empty;
	}

	public void Create(string title, string content, string createdBy)
	{
		Id = SequentialGuid.SequentialGuidGenerator.Instance.NewGuid();
		Title = title;
		Content = content;
		CreatedBy = createdBy;
		CreatedAt = DateTime.UtcNow;
		Status = AnnouncementStatus.Draft;

		AddDomainEvent(new AnnouncementCacheDomainEvent(Id, Status));
	}

	public void Update(string title, string content)
	{
		if (Status == AnnouncementStatus.Published)
			throw new InvalidOperationException("Cannot update published announcement directly; unpublish or create a new version.");

		Title = title ?? Title;
		Content = content ?? Content;

		AddDomainEvent(new AnnouncementCacheDomainEvent(Id, Status));
	}

	public void Publish(DateTime publishedAt, DateTime? expiresAt = null)
	{
		if (Status != AnnouncementStatus.Draft)
			throw new InvalidOperationException("Only draft announcements can be published.");

		Status = AnnouncementStatus.Published;
		PublishedAt = publishedAt;
		ExpiresAt = expiresAt;

		AddDomainEvent(new AnnouncementCacheDomainEvent(Id, Status));
	}

	public void Archive()
	{
		if (Status != AnnouncementStatus.Published)
			throw new InvalidOperationException("Only published announcements can be archived.");

		Status = AnnouncementStatus.Archived;

		AddDomainEvent(new AnnouncementCacheDomainEvent(Id, Status));
	}

	public void Pin() => IsPinned = true;
	public void Unpin() => IsPinned = false;

	public void SoftDelete()
	{
		Status = AnnouncementStatus.SoftDelete;
		IsDeleted = true;

		AddDomainEvent(new AnnouncementCacheDomainEvent(Id, Status));
	}
	public void Restore() => IsDeleted = false;

	public bool IsExpired(DateTime now)
		=> ExpiresAt.HasValue && ExpiresAt.Value <= now;

	private void AddDomainEvent(IDomainEvent @event)
	{
		_domainEvents.Add(@event);
	}

	public void ClearDomainEvents()
	{
		_domainEvents.Clear();
	}
}
