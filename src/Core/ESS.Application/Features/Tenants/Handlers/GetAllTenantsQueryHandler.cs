using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ESS.Application.Common.Models;
using ESS.Application.Common.Interfaces;
using ESS.Application.Features.Tenants.Queries;
using ESS.Application.Features.Tenants.DTOs;

namespace ESS.Application.Features.Tenants.Handlers;

public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, Result<IEnumerable<TenantDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllTenantsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<TenantDto>>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _context.Tenants
            .Include(t => t.Domains)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var tenantDtos = _mapper.Map<IEnumerable<TenantDto>>(tenants);
        return Result.Success(tenantDtos);
    }
}