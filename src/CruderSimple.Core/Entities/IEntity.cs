﻿using CruderSimple.Core.ViewModels;

namespace CruderSimple.Core.Entities;

public interface IEntity
{
    public string Id { get; set; }
    public IEntity FromInput(BaseDto input);
    BaseDto ConvertToOutput();
    public string GetPrimaryKey();
}
