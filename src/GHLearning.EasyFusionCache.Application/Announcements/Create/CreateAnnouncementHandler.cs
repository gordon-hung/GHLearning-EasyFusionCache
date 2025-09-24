using GHLearning.EasyFusionCache.Domain.Announcements;
using GHLearning.EasyFusionCache.SharedKernel;
using MediatR;

namespace GHLearning.EasyFusionCache.Application.Announcements.Create;
internal class CreateAnnouncementHandler(
	IAnnouncementRepository repo,
	IDomainEventDispatcher dispatcher) : IRequestHandler<CreateAnnouncementCommand, Guid>
{
	public async Task<Guid> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
	{
		var announcement = new Announcement();
		announcement.Create(request.Title, request.Content, request.CreatedBy);
		await repo.AddAsync(announcement, cancellationToken).ConfigureAwait(false);
		await dispatcher.DispatchAsync(announcement.DomainEvents, cancellationToken).ConfigureAwait(false);
		return announcement.Id;
	}
}
