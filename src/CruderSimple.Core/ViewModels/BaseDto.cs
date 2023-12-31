﻿using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CruderSimple.Core.ViewModels;

public class BaseDto : IComparable<BaseDto>
{
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

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
