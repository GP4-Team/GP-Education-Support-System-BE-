using ESS.Domain.Common;
using ESS.Domain.Interfaces;

namespace ESS.Domain.Entities.Users;

public class User : BaseEntity, ITenantEntity
{
    public User(string username, string email, string firstName, string lastName, string tenantId)
    {
        Id = Guid.NewGuid();
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        TenantId = tenantId;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    private User() { }

    public new Guid Id { get; set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public string TenantId { get; set; } = string.Empty;

    public void Update(string username, string email, string firstName, string lastName)
    {
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }
}