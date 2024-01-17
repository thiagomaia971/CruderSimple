using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter(nameof(TEntity))]
[CascadingTypeParameter(nameof(TItem))]
public partial class CruderDateColumn<TEntity, TItem> : CruderColumnBase<TEntity, TItem>
    where TEntity : IEntity
    where TItem : BaseDto
{

}
