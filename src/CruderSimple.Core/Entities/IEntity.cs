using CruderSimple.Core.ViewModels;

namespace CruderSimple.Core.Entities;

public interface IEntity
{
    public IEntity FromInput(InputDto input);
    public OutputDto ToOutput();
}
