using GHLearning.EasyFusionCache.Application.Announcements.Archive;
using GHLearning.EasyFusionCache.Application.Announcements.Create;
using GHLearning.EasyFusionCache.Application.Announcements.Get;
using GHLearning.EasyFusionCache.Application.Announcements.Publish;
using GHLearning.EasyFusionCache.Application.Announcements.Query;
using GHLearning.EasyFusionCache.Application.Announcements.SoftDelete;
using GHLearning.EasyFusionCache.Domain.Announcements;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GHLearning.EasyFusionCache.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnnouncementsController(
	IMediator mediator) : ControllerBase
{

	// -------------------
	// Create Announcement
	// -------------------
	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateRequest req)
	{
		var id = await mediator.Send(new CreateAnnouncementCommand(req.Title, req.Content, req.CreatedBy), HttpContext.RequestAborted).ConfigureAwait(false);
		return CreatedAtAction(nameof(GetById), new { id }, new { id });
	}

	// -------------------
	// Get Paged Announcements
	// -------------------
	[HttpGet()]
	public async Task<IActionResult> GetPaged([FromQuery] AnnouncementStatus status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
	{
		var (items, total) = await mediator.Send(new QueryAnnouncementsQuery(status, page, pageSize, search), HttpContext.RequestAborted).ConfigureAwait(false);
		return Ok(new { items, total });
	}

	// -------------------
	// Get Announcement by Id
	// -------------------
	[HttpGet("{id}")]
	public async Task<IActionResult> GetById(Guid id)
	{
		var result = await mediator.Send(new GetAnnouncementByIdQuery(id), HttpContext.RequestAborted).ConfigureAwait(false);
		return result == null ? NotFound() : Ok(result);
	}

	// -------------------
	// Publish Announcement
	// -------------------
	[HttpPost("{id}/publish")]
	public async Task<IActionResult> Publish(Guid id, [FromBody] PublishRequest req)
	{
		await mediator.Send(new PublishAnnouncementCommand(id, req.PublishedAt, req.ExpiresAt), HttpContext.RequestAborted).ConfigureAwait(false);
		return NoContent();
	}

	// -------------------
	// Archive Announcement
	// -------------------
	[HttpPost("{id}/archive")]
	public async Task<IActionResult> Archive(Guid id)
	{
		await mediator.Send(new ArchiveAnnouncementCommand(id), HttpContext.RequestAborted).ConfigureAwait(false);
		return NoContent();
	}

	// -------------------
	// Soft Delete Announcement
	// -------------------
	[HttpDelete("{id}")]
	public async Task<IActionResult> SoftDelete(Guid id)
	{
		await mediator.Send(new SoftDeleteAnnouncementCommand(id), HttpContext.RequestAborted).ConfigureAwait(false);
		return NoContent();
	}

	// -------------------
	// Request DTOs
	// -------------------
	public class CreateRequest
	{
		public required string Title { get; set; }
		public required string Content { get; set; }
		public required string CreatedBy { get; set; }
	}

	public class PublishRequest
	{
		public required DateTime PublishedAt { get; set; }
		public required DateTime? ExpiresAt { get; set; }
	}
}