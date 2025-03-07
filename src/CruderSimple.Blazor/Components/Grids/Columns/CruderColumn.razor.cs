using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids.Columns;

[CascadingTypeParameter(nameof(TColumnEntity))]
[CascadingTypeParameter(nameof(TColumnDto))]
public partial class CruderColumn<TColumnEntity, TColumnDto> : CruderColumnBase<TColumnEntity, TColumnDto>
    where TColumnEntity : IEntity
    where TColumnDto : BaseDto
{
    public DataGridColumn<TColumnDto> DataGridColumn { get; set; }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (DataGridColumn != null && GridSort != null)
            DataGridColumn.SortField = GridSort;

        if (DataGridColumn != null && DataGridColumn.ParentDataGrid != null)
            Events = (CruderGridEvents<TColumnDto>)DataGridColumn.ParentDataGrid.Attributes["Events"];

        return base.OnAfterRenderAsync(firstRender);
    }

    private void ValueChanged(CellEditContext<TColumnDto> cellEdit, double value)
    {
        cellEdit.UpdateCell(ColumnField, value);
    }

    private string GetCurrencyFormat(TColumnDto context)
    {
        return $"R$ {context.GetValueByPropertyName(ColumnField)}";
    }
}
