using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids.Columns;

[CascadingTypeParameter(nameof(TValue))]
[CascadingTypeParameter(nameof(TColumnEntity))]
[CascadingTypeParameter(nameof(TColumnDto))]
public partial class CruderNumericColumn<TValue, TColumnEntity, TColumnDto> : CruderColumnBase<TColumnEntity, TColumnDto>
    where TValue : struct, IComparable, IConvertible, IFormattable
    where TColumnEntity : IEntity
    where TColumnDto : BaseDto
{
    public DataGridNumericColumn<TColumnDto> DataGridNumericColumnRef { get; set; }
    public object Value { get; set; }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (DataGridNumericColumnRef != null && GridSort != null)
            DataGridNumericColumnRef.SortField = GridSort;

        if (DataGridNumericColumnRef != null && DataGridNumericColumnRef.ParentDataGrid != null)
            Events = (CruderGridEvents<TColumnDto>) DataGridNumericColumnRef.ParentDataGrid.Attributes["Events"];

        return base.OnAfterRenderAsync(firstRender);
    }
}
