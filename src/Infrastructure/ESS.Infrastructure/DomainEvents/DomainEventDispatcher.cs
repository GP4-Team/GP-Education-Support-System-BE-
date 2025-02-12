using ESS.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESS.Infrastructure.DomainEvents;

public class DomainEventDispatcher
{
    private readonly ILogger<DomainEventDispatcher> _logger;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(
        ILogger<DomainEventDispatcher> logger,
        IMediator mediator,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _mediator = mediator;
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchEventsAsync(IEnumerable<IHasDomainEvents> entities, CancellationToken cancellationToken = default)
    {
        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .OrderBy(e => e.OccurredOn)
            .ToList();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        _logger.LogInformation("Dispatching {Count} domain events", domainEvents.Count);

        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation("Dispatching domain event {Event}", domainEvent.GetType().Name);

            try
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching domain event {Event}", domainEvent.GetType().Name);
                throw;
            }
        }
    }
}