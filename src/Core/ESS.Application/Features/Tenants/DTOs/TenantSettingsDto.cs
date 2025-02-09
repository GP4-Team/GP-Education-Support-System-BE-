namespace ESS.Application.Features.Tenants.DTOs;
public record TenantSettingsDto
{
    public Guid Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}
