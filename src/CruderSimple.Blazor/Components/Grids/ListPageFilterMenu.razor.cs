using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter(nameof(TEntity))]
[CascadingTypeParameter(nameof(TDto))]
public partial class ListPageFilterMenu<TEntity, TDto> : ComponentBase
    where TEntity : IEntity
    where TDto : BaseDto
{

    [CascadingParameter] public DataGrid<TDto> ParentDataGrid { get; set; }

    [Parameter] public DataGridColumn<TDto> Column { get; set; }

    public DataGridSelectColumn<TDto> SelectColumn => (DataGridSelectColumn<TDto>)Column;
    public object SelectColumnCrudService
    {
        get
        {
            if (SelectColumn is null)
                return null;
            if (SelectColumn.Attributes.ContainsKey("Service"))
                return SelectColumn.Attributes["Service"];
            return null;
        }
    }

    public string SearchValue
    {
        get =>
            ((ListPageFilter)Column.Filter.SearchValue)?.SearchValue;
        set
        {
            if (Column.Filter.SearchValue is null)
                InitializeSearchValue();
            ((ListPageFilter)Column.Filter.SearchValue).SearchValue = value;
        }
    }

    public DataGridColumnFilterMethod FilterMethod
    {
        get =>
            ((ListPageFilter)Column.Filter.SearchValue)?.FilterMethod ?? DataGridColumnFilterMethod.Contains;
        set
        {
            if (Column.Filter.SearchValue is null)
                InitializeSearchValue();
            ((ListPageFilter)Column.Filter.SearchValue).FilterMethod = value;
        }
    }

    public object SelectItem
    {
        get =>
            ((ListPageFilter)Column.Filter.SearchValue)?.SelectItem is null ? null : JsonConvert.DeserializeObject(((ListPageFilter)Column.Filter.SearchValue)?.SelectItem);
        set
        {
            if (value == null)
            {
                Column.Filter.SearchValue = null;
                return;
            }
            if (Column.Filter.SearchValue is null)
                InitializeSearchValue();
            ((ListPageFilter)Column.Filter.SearchValue).SelectItem = value == null ? null : JsonConvert.SerializeObject(value);
        }
    }

    public bool IsNumeric => Column.ColumnType == DataGridColumnType.Numeric;
    public bool IsDate => Column.ColumnType == DataGridColumnType.Date;
    public bool IsSelect => Column.ColumnType == DataGridColumnType.Select;
    public DataGridSelectColumn<TDto> DataGridSelectColumn => Column as DataGridSelectColumn<TDto>;
    public RenderFragment SelectRender { get; set; }

    protected override Task OnInitializedAsync()
    {
        base.InvokeAsync(() =>
        {
            var events = (CruderGridEvents<TDto>) ParentDataGrid.Attributes["Events"];
            events.OnColumnsLoaded += () =>
            {
                if (IsSelect && SelectRender is null)
                    SelectRender = GenerateSelectComponent(SelectItem);
            };
        });

        return base.OnInitializedAsync();
    }

    private void InitializeSearchValue()
    {
        Column.Filter.SearchValue = new ListPageFilter
        {
            FilterMethod = DataGridColumnFilterMethod.Contains
        };
    }

    private void DateChanged(bool start, DateTime? date)
    {
        var searchValueSplited = SearchValue is null ? new string[2] : SearchValue?.ToString().Split("_");
        if (start)
            searchValueSplited[0] = date.Value.ToStartDate().ToString("O");
        else
            searchValueSplited[1] = date.Value.ToEndDate().ToString("O");

        SearchValue = string.Join("_", searchValueSplited);
    }

    private DateTime? GetDate(bool start)
    {
        if (SearchValue is null)
            return null;
        var values = SearchValue.Split("_");
        if (string.IsNullOrEmpty(values[start ? 0 : 1]))
            return null;
        return DateTime.Parse(values[start ? 0 : 1]);
    }

    private async Task Filter()
    {
        await ParentDataGrid.Refresh();
        await ParentDataGrid.Reload();
        SelectRender = GenerateSelectComponent(SelectItem);
    }

    private async Task Clear()
    {
        Column.Filter.SearchValue = null;
        await ParentDataGrid.Refresh();
        await ParentDataGrid.Reload();
        SelectRender = null;
        StateHasChanged();
        SelectRender = GenerateSelectComponent(null);
        StateHasChanged();
    }

    private RenderFragment GenerateSelectComponent(object selectItem)
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
            selectItem, 
            SelectChanged, 
            false, 
            DataGridSelectColumn.Attributes);
        return render;
    }

    public async Task SelectChanged((string Key, object Value) value)
    {
        if (string.IsNullOrEmpty(value.Key))
            return;
        SearchValue = value.Key;
        SelectItem = value.Value;
        SelectRender = GenerateSelectComponent(SelectItem);
        await Filter();
    }
}
