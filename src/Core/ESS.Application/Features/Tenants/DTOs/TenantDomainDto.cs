namespace ESS.Application.Features.Tenants.DTOs;
public record TenantDomainDto
{
    public Guid Id { get; init; }
    public string Domain { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}