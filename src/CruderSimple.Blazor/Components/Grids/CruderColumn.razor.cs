using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter(nameof(TEntity))]
[CascadingTypeParameter(nameof(TItem))]
public partial class CruderColumn<TEntity, TItem> : CruderColumnBase<TEntity, TItem>
    where TEntity : IEntity
    where TItem : BaseDto
{
    public DataGridColumn<TItem> DataGridColumn { get; set; }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (DataGridColumn != null && GridSort != null)
            DataGridColumn.SortField = GridSort;

        if (DataGridColumn != null && DataGridColumn.ParentDataGrid != null)
            Events = (CruderGridEvents<TItem>)DataGridColumn.ParentDataGrid.Attributes["Events"];

        return base.OnAfterRenderAsync(firstRender);
    }

    private void ValueChanged(CellEditContext<TItem> cellEdit, double value)
    {
        cellEdit.UpdateCell(ColumnField, value);
    }

    private string GetCurrencyFormat(TItem context)
    {
        return $"R$ {context.GetValueByPropertyName(ColumnField)}";
    }
}
