using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ESS.Application.Common.Models;
using ESS.Application.Common.Interfaces;
using ESS.Application.Features.Tenants.Queries;
using ESS.Application.Features.Tenants.DTOs;

namespace ESS.Application.Features.Tenants.Handlers;

public class GetTenantByIdentifierQueryHandler : IRequestHandler<GetTenantByIdentifierQuery, Result<TenantDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTenantByIdentifierQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TenantDto>> Handle(GetTenantByIdentifierQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Domains)
            .FirstOrDefaultAsync(t => t.Identifier == request.Identifier, cancellationToken);

        if (tenant == null)
        {
            return Result.Failure<TenantDto>($"Tenant with identifier '{request.Identifier}' not found");
        }

        var tenantDto = _mapper.Map<TenantDto>(tenant);
        return Result.Success(tenantDto);
    }
}