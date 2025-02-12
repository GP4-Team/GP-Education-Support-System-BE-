namespace ESS.Domain.Enums;

public enum TenantDatabaseStatus
{
    /// <summary>
    /// Database creation is pending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Database is currently being created
    /// </summary>
    Creating = 1,

    /// <summary>
    /// Database is active and ready for use
    /// </summary>
    Active = 2,

    /// <summary>
    /// Database creation failed
    /// </summary>
    Failed = 3
}