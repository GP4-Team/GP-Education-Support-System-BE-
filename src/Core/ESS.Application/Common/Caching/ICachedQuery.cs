using MediatR;

namespace ESS.Application.Common.Caching;

public interface ICachedQuery<TResponse> : IRequest<TResponse>, ICacheKey
{
}