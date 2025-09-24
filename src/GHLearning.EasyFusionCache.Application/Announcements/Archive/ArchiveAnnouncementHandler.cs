using GHLearning.EasyFusionCache.Domain.Announcements;
using GHLearning.EasyFusionCache.SharedKernel;
using MediatR;

namespace GHLearning.EasyFusionCache.Application.Announcements.Archive;
internal class ArchiveAnnouncementHandler(
	IAnnouncementRepository repo,
	IDomainEventDispatcher dispatcher) : IRequestHandler<ArchiveAnnouncementCommand>
{
	public async Task Handle(ArchiveAnnouncementCommand request, CancellationToken cancellationToken)
	{
		var announcement = await repo.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false)
			?? throw new Exception("Announcement not found");

		announcement.Archive();

		await repo.UpdateAsync(announcement, cancellationToken).ConfigureAwait(false);

		await dispatcher.DispatchAsync(announcement.DomainEvents, cancellationToken).ConfigureAwait(false);
	}
}
