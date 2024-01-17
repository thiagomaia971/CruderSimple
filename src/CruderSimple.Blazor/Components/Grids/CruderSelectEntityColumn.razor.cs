using Blazorise.DataGrid;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter(nameof(TEntity))]
[CascadingTypeParameter(nameof(TItem))]
[CascadingTypeParameter(nameof(TColumnItem))]
public partial class CruderSelectEntityColumn<TEntity, TItem, TColumnItem> : CruderColumnBase<TEntity, TItem>
    where TEntity : IEntity
    where TItem : BaseDto
    where TColumnItem : BaseDto
{
    /// <summary>
    /// Property used to search on Grid component inside of Filter
    /// </summary>
    [Parameter] public string GridSearchKey { get; set; }

    /// <summary>
    /// Selects properties to search on Select component request inside of Filter
    /// </summary>
    [Parameter] public string SelectSearchKey { get; set; }

    /// <summary>
    /// Custom select request params
    /// </summary>

    [Inject] public ICrudService<TEntity, TColumnItem> CrudService { get; set; }
    public DataGridSelectColumn<TItem> DataGridSelectColumn { get; set; }

    public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    public RenderFragment SelectComponent { get; set; }

    protected override Task OnInitializedAsync()
    {
        Attributes.Add("Service", CrudService);
        Attributes.Add("SearchKey", SelectSearchKey);
        Attributes.Add("Field", GridSearchKey);
        return base.OnInitializedAsync();
    }

    private RenderFragment CreateSelectComponent(CellEditContext<TItem> cellEdit)
    {
        if (DataGridSelectColumn is null)
            return null;

        var service = DataGridSelectColumn.Attributes["Service"];
        if (service is null)
        {
            Console.WriteLine("Service is null");
            return null;
        }

        var entity = service.GetType().GenericTypeArguments[0];
        var entityDto = service.GetType().GenericTypeArguments[1];

        var render = EntityAutocompleteUtils.CreateComponent(
            entity,
            entityDto,
            cellEdit.CellValue,
            async ((string Key, object Value) value) => await SelectChanged(value, cellEdit),
            false,
            DataGridSelectColumn.Attributes);
        StateHasChanged();
        return render;
    }

    public async Task SelectChanged((string Key, object Value) value, CellEditContext<TItem> cellEdit)
    {
        cellEdit.UpdateCell(ColumnField, value.Value);
    }

    protected string GetGridName(TItem item)
    { 
        var itemProperties = item.GetType().GetProperty(ColumnField);
        if (itemProperties is null)
            return item.GetValue;
        return (itemProperties.GetValue(item) as TColumnItem)?.GetValue ?? string.Empty;
    }
}
