namespace ESS.Application.Features.Tenants.DTOs;

public record TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUpdatedAt { get; init; }
    public ICollection<TenantDomainDto> Domains { get; init; } = new List<TenantDomainDto>();
    public ICollection<TenantSettingsDto> Settings { get; init; } = new List<TenantSettingsDto>();
}