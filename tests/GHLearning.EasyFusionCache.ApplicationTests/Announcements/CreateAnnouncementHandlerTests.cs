using NSubstitute;
using GHLearning.EasyFusionCache.Application.Announcements.Create;
using GHLearning.EasyFusionCache.Domain.Announcements;
using GHLearning.EasyFusionCache.SharedKernel;

namespace GHLearning.EasyFusionCache.ApplicationTests.Announcements;

public class CreateAnnouncementHandlerTests
{
	[Fact]
	public async Task Handle_ShouldCreateAnnouncement_AndDispatchEvents_AndReturnId()
	{
		// Arrange
		var fakeRepo = Substitute.For<IAnnouncementRepository>();
		var fakeDispatcher = Substitute.For<IDomainEventDispatcher>();
		var sut = new CreateAnnouncementHandler(fakeRepo, fakeDispatcher);

		var command = new CreateAnnouncementCommand("標題", "內容", "作者");

		Announcement? addedAnnouncement = null;
		fakeRepo.AddAsync(Arg.Do<Announcement>(a => addedAnnouncement = a), Arg.Any<CancellationToken>())
			.Returns(Task.CompletedTask);

		// Act
		var resultId = await sut.Handle(command, CancellationToken.None);

		// Assert
		Assert.NotNull(addedAnnouncement);
		Assert.Equal(command.Title, addedAnnouncement.Title);
		Assert.Equal(command.Content, addedAnnouncement.Content);
		Assert.Equal(command.CreatedBy, addedAnnouncement.CreatedBy);
		Assert.Equal(AnnouncementStatus.Draft, addedAnnouncement.Status);
		Assert.Equal(resultId, addedAnnouncement.Id);
		await fakeRepo.Received(1).AddAsync(addedAnnouncement, Arg.Any<CancellationToken>());
		await fakeDispatcher.Received(1).DispatchAsync(Arg.Any<IEnumerable<IDomainEvent>>(), Arg.Any<CancellationToken>());
	}
}