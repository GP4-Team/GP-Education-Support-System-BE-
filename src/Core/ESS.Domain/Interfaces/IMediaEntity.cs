// src/Core/ESS.Domain/Interfaces/IMediaEntity.cs
namespace ESS.Domain.Interfaces;

public interface IMediaEntity : ITenantEntity
{
    string FilePath { get; }
    bool IsTemporary { get; }
    Guid? TempGuid { get; }
}