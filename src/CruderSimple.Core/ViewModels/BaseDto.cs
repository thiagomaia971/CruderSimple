using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CruderSimple.Core.ViewModels;

public abstract class BaseDto(string Id) : IComparable<BaseDto>
{
    public string Id { get; set; } = Id;

    public virtual string GetKey => Id;
    
    public virtual string GetValue => Id;

    public int CompareTo(BaseDto? other)
    {
        return Id == other.Id ? 1 : 0;
    }

    public override string ToString()
    {
        return GetKey;
    }
}
