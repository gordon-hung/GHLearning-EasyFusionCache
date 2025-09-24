namespace GHLearning.EasyFusionCache.SharedKernel;
public interface IDomainEvent
{
	DateTimeOffset OccurredOn { get; }
}
