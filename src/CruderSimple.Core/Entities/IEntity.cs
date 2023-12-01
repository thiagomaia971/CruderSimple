using CruderSimple.Core.ViewModels;

namespace CruderSimple.Core.Entities;

public interface IEntity
{
    public string Id { get; set; }
    public IEntity FromInput(InputDto input);
    public OutputDto ToOutput();

    public string GetPrimaryKey();
}
