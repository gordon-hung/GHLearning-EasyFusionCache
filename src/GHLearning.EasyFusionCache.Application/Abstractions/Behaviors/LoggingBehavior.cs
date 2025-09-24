using MediatR;
using Microsoft.Extensions.Logging;

namespace GHLearning.EasyFusionCache.Application.Abstractions.Behaviors;
public class LoggingBehavior<TRequest, TResponse>(
	ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		logger.LogInformation("Handling {RequestName} with data {@Request}", typeof(TRequest).Name, request);
		var response = await next(cancellationToken).ConfigureAwait(false);
		logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
		return response;
	}
}
