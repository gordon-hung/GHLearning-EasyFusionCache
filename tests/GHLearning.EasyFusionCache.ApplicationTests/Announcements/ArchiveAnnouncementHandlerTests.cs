using GHLearning.EasyFusionCache.Application.Announcements.Archive;
using GHLearning.EasyFusionCache.Domain.Announcements;
using GHLearning.EasyFusionCache.SharedKernel;
using NSubstitute;

namespace GHLearning.EasyFusionCache.ApplicationTests.Announcements;

public class ArchiveAnnouncementHandlerTests
{
	[Fact]
	public async Task Handle_ShouldArchiveAnnouncement_AndDispatchEvents()
	{
		// Arrange
		var fakeRepo = Substitute.For<IAnnouncementRepository>();
		var fakeDispatcher = Substitute.For<IDomainEventDispatcher>();
		var sut = new ArchiveAnnouncementHandler(fakeRepo, fakeDispatcher);

		var announcement = Substitute.ForPartsOf<Announcement>();
		announcement.Status = AnnouncementStatus.Published;
		var command = new ArchiveAnnouncementCommand(Guid.NewGuid());

		fakeRepo.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
			.Returns(announcement);

		// Act
		await sut.Handle(command, CancellationToken.None);

		// Assert
		await fakeRepo.Received(1).UpdateAsync(announcement, Arg.Any<CancellationToken>());
		await fakeDispatcher.Received(1).DispatchAsync(Arg.Any<IEnumerable<IDomainEvent>>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ShouldThrowException_WhenAnnouncementNotFound()
	{
		// Arrange
		var fakeRepo = Substitute.For<IAnnouncementRepository>();
		var fakeDispatcher = Substitute.For<IDomainEventDispatcher>();
		var sut = new ArchiveAnnouncementHandler(fakeRepo, fakeDispatcher);

		var command = new ArchiveAnnouncementCommand(Guid.NewGuid());
		fakeRepo.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
			.Returns((Announcement?)null);

		// Act & Assert
		await Assert.ThrowsAsync<Exception>(() => sut.Handle(command, CancellationToken.None));
	}
}