using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CruderSimple.Core.ViewModels;

public class BaseDto : IComparable<BaseDto>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual string GetKey => Id;
    
    public virtual string GetValue => Id;

    public BaseDto()
    {
    }

    public BaseDto(string id, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public int CompareTo(BaseDto? other) 
        => Id == other.Id ? 1 : 0;
}
