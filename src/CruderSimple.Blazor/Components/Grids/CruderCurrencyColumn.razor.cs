using Blazorise;
using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter(nameof(TEntity))]
[CascadingTypeParameter(nameof(TItem))]
public partial class CruderCurrencyColumn<TEntity, TItem> : CruderColumnBase<TEntity, TItem>
    where TEntity : IEntity
    where TItem : BaseDto
{
    public DataGridNumericColumn<TItem> DataGridNumericColumn { get; set; }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (DataGridNumericColumn != null && GridSort != null)
            DataGridNumericColumn.SortField = GridSort;

        if (DataGridNumericColumn != null && DataGridNumericColumn.ParentDataGrid != null)
            Events = (CruderGridEvents<TItem>) DataGridNumericColumn.ParentDataGrid.Attributes["Events"];

        return base.OnAfterRenderAsync(firstRender);
    }
}
