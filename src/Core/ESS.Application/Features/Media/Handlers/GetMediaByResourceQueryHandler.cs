// src/Core/ESS.Application/Features/Media/Handlers/GetMediaByResourceQueryHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ESS.Application.Common.Models;
using ESS.Application.Common.Interfaces;
using ESS.Application.Features.Media.DTOs;
using ESS.Application.Features.Media.Queries;

namespace ESS.Application.Features.Media.Handlers;

public class GetMediaByResourceQueryHandler :
    IRequestHandler<GetMediaByResourceQuery, Result<IEnumerable<MediaDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;

    public GetMediaByResourceQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ITenantService tenantService)
    {
        _context = context;
        _mapper = mapper;
        _tenantService = tenantService;
    }

    public async Task<Result<IEnumerable<MediaDto>>> Handle(
        GetMediaByResourceQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var query = _context.Media
            .Where(m => m.TenantId == tenantId
                && m.ResourceId == request.ResourceId
                && m.ResourceType == request.ResourceType
                && !m.IsTemporary);

        if (!string.IsNullOrEmpty(request.Collection))
        {
            query = query.Where(m => m.Collection == request.Collection);
        }

        var media = await query.ToListAsync(cancellationToken);

        // src/Core/ESS.Application/Features/Media/Handlers/GetMediaByResourceQueryHandler.cs
        // Inside Handle method:
        return Result.Success(_mapper.Map<IEnumerable<MediaDto>>(media));
    }
}