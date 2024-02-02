using Blazorise;
using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter(nameof(TColumnEntity))]
[CascadingTypeParameter(nameof(TColumnDto))]
public partial class CruderCurrencyColumn<TColumnEntity, TColumnDto> : CruderColumnBase<TColumnEntity, TColumnDto>
    where TColumnEntity : IEntity
    where TColumnDto : BaseDto
{
    public DataGridNumericColumn<TColumnDto> DataGridNumericColumn { get; set; }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (DataGridNumericColumn != null && GridSort != null)
            DataGridNumericColumn.SortField = GridSort;

        if (DataGridNumericColumn != null && DataGridNumericColumn.ParentDataGrid != null)
            Events = (CruderGridEvents<TColumnDto>) DataGridNumericColumn.ParentDataGrid.Attributes["Events"];

        return base.OnAfterRenderAsync(firstRender);
    }
}
