using GHLearning.EasyFusionCache.Domain.Announcements;
using GHLearning.EasyFusionCache.SharedKernel;
using MediatR;

namespace GHLearning.EasyFusionCache.Application.Announcements.SoftDelete;
internal class SoftDeleteAnnouncementHandler(
	IAnnouncementRepository repo,
	IDomainEventDispatcher dispatcher) : IRequestHandler<SoftDeleteAnnouncementCommand>
{
	public async Task Handle(SoftDeleteAnnouncementCommand request, CancellationToken cancellationToken)
	{
		var announcement = await repo.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false)
			?? throw new Exception("Announcement not found");

		announcement.SoftDelete();
		await repo.SoftDeleteAsync(announcement, cancellationToken).ConfigureAwait(false);
		await dispatcher.DispatchAsync(announcement.DomainEvents, cancellationToken).ConfigureAwait(false);
	}
}