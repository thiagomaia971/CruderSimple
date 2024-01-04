using CruderSimple.Core.Entities;

namespace CruderSimple.Core.Interfaces;

public interface IUser : IEntity
{
    List<string> GetPermissions();
}