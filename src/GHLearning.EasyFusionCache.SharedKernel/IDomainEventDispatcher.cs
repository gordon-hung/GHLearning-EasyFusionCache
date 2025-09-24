namespace GHLearning.EasyFusionCache.SharedKernel;
public interface IDomainEventDispatcher
{
	Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
