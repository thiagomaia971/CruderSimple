using CruderSimple.Core.ViewModels;
using Mapster;

namespace CruderSimple.Core.Entities;

public interface IEntity
{
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public void DeleteMethod(int modifiedBy);
    public IEntity FromInput(BaseDto input);
    BaseDto ConvertToOutput();
    public string GetPrimaryKey();
}
