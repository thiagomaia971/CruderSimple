using Blazorise.DataGrid;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Mapster;
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

    private DataGridSelectColumn<TItem> DataGridSelectColumn { get; set; }
    private DataGrid<TItem> DataGrid => DataGridSelectColumn?.ParentDataGrid;
    public TItem CurrentSelect { get; set; }

    public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    public RenderFragment SelectComponent { get; set; }
    private bool Loaded { get; set; }

    protected override Task OnInitializedAsync()
    {
        Attributes.Add("Service", CrudService);
        Attributes.Add("SearchKey", SelectSearchKey);
        Attributes.Add("Field", GridSearchKey);
        return base.OnInitializedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (DataGridSelectColumn != null && GridSort != null)
            DataGridSelectColumn.SortField = GridSort;

        if (DataGridSelectColumn != null && DataGridSelectColumn.ParentDataGrid != null)
            Events = (CruderGridEvents<TItem>)DataGridSelectColumn.ParentDataGrid.Attributes["Events"];

        //if (SelectComponent == null && AlwaysEditable)
        //{
        //    SelectComponent = CreateSelectComponent();
        //    StateHasChanged();
        //}
        if (!Loaded && DataGrid != null)
            OnDataGridLoaded();
        base.OnAfterRender(firstRender);
    }

    protected void OnDataGridLoaded()
    {
        Loaded = true;
        Events.OnEditMode += () =>
        {
            SelectComponent = CreateSelectComponent(DataGrid.ReadCellEditValue(ColumnField));
            StateHasChanged();
        };
    }

    private RenderFragment CreateSelectComponent(object value)
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
            value,
            async ((string Key, object Value) value) => await SelectChanged(value/*, cellEdit*/),
            false,
            DataGridSelectColumn.Attributes);
        StateHasChanged();
        return render;
    }

    public async Task SelectChanged((string Key, object Value) value/*, CellEditContext<TItem> cellEdit*/)
    {
        OldValue = CurrentSelect.Adapt<TItem>();
        CurrentSelect.SetValueByPropertyName(value.Value, ColumnField);
        NewValue = CurrentSelect;

        DataGrid.UpdateCellEditValue(ColumnField, value.Value);
        await OnBlur();
        SelectComponent = CreateSelectComponent(DataGrid.ReadCellEditValue(ColumnField));
    }

    protected string GetGridName(TItem item)
    { 
        var itemProperties = item.GetType().GetProperty(ColumnField);
        if (itemProperties is null)
            return item.GetValue;
        return (itemProperties.GetValue(item) as TColumnItem)?.GetValue ?? string.Empty;
    }

    protected override async Task OnClick(TItem item)
    {
        CurrentSelect = item;
        await base.OnClick(item);
    }
}
