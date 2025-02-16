namespace ESS.Domain.Enums;

public enum TenantDatabaseStatus
{
    Pending = 0,
    Creating = 1,
    Active = 2,
    Failed = 3,
    Disabled = 4,
    Migrating = 5,
    MigrationFailed = 6
}