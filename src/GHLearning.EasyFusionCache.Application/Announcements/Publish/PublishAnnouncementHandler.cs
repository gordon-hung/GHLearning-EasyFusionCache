using GHLearning.EasyFusionCache.Domain.Announcements;
using GHLearning.EasyFusionCache.SharedKernel;
using MediatR;

namespace GHLearning.EasyFusionCache.Application.Announcements.Publish;
internal class PublishAnnouncementHandler(
	IAnnouncementRepository repo,
	IDomainEventDispatcher dispatcher) : IRequestHandler<PublishAnnouncementCommand>
{
	public async Task Handle(PublishAnnouncementCommand request, CancellationToken cancellationToken)
	{
		var announcement = await repo.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false)
			?? throw new Exception("Announcement not found");

		announcement.Publish(request.PublishedAt, request.ExpiresAt);
		await repo.UpdateAsync(announcement, cancellationToken).ConfigureAwait(false);
		await dispatcher.DispatchAsync(announcement.DomainEvents, cancellationToken).ConfigureAwait(false);
	}
}
