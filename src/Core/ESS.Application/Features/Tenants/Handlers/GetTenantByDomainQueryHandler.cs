using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ESS.Application.Common.Models;
using ESS.Application.Common.Interfaces;
using ESS.Application.Features.Tenants.Queries;
using ESS.Application.Features.Tenants.DTOs;

namespace ESS.Application.Features.Tenants.Handlers;

public class GetTenantByDomainQueryHandler : IRequestHandler<GetTenantByDomainQuery, Result<TenantDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTenantByDomainQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TenantDto>> Handle(GetTenantByDomainQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _context.TenantDomains
            .Include(td => td.Tenant)
            .ThenInclude(t => t!.Domains)
            .Where(td => td.Domain == request.Domain && td.IsActive && td.Tenant!.IsActive)
            .Select(td => td.Tenant)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant == null)
        {
            return Result.Failure<TenantDto>($"No active tenant found for domain '{request.Domain}'");
        }

        var tenantDto = _mapper.Map<TenantDto>(tenant);
        return Result.Success(tenantDto);
    }
}