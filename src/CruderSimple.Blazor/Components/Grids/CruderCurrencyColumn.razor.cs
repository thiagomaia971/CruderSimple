using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
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
    private void ValueChanged(CellEditContext<TItem> cellEdit, double value)
    {
        cellEdit.UpdateCell(ColumnField, value);
    }

    private string GetCurrencyFormat(TItem context)
    {
        return $"R$ {context.GetValueByPropertyName(ColumnField)}";
    }
}
