using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
namespace ESS.Application.Common.Behaviors;
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var uniqueId = Guid.NewGuid().ToString();

        _logger.LogInformation("Begin Request {UniqueId}: {Name} {@Request}",
            uniqueId, requestName, request);

        var timer = new Stopwatch();
        timer.Start();

        var response = await next();

        timer.Stop();

        _logger.LogInformation("End Request {UniqueId}: {Name} ({ElapsedMilliseconds}ms)",
            uniqueId, requestName, timer.ElapsedMilliseconds);

        return response;
    }
}