using MediatR;

namespace ESS.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
