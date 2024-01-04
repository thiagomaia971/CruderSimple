using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CruderSimple.Core.ViewModels;

public class BaseDto(string id, DateTime createdAt, DateTime? updatedAt) : IComparable<BaseDto>
{
    public string Id { get; set; } = id;
    public DateTime CreatedAt { get; set; } = createdAt;
    public DateTime? UpdatedAt { get; set; } = updatedAt;

    public virtual string GetKey => Id;
    
    public virtual string GetValue => Id;

    public int CompareTo(BaseDto? other) 
        => Id == other.Id ? 1 : 0;
}
