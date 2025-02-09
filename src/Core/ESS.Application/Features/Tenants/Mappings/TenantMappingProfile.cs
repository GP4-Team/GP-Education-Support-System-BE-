using AutoMapper;
using ESS.Application.Features.Tenants.DTOs;
using ESS.Domain.Entities;

namespace ESS.Application.Features.Tenants.Mappings;

public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<Tenant, TenantDto>();
        CreateMap<TenantDomain, TenantDomainDto>();
        CreateMap<TenantSettings, TenantSettingsDto>();
    }
}