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
            ((ListPageFilter)Column.Filter.SearchValue)?.SelectItem;
        set
        {
            if (Column.Filter.SearchValue is null)
                InitializeSearchValue();
            ((ListPageFilter)Column.Filter.SearchValue).SelectItem = value;
        }
    }

    public bool IsNumeric => Column.ColumnType == DataGridColumnType.Numeric;
    public bool IsDate => Column.ColumnType == DataGridColumnType.Date;
    public bool IsSelect => Column.ColumnType == DataGridColumnType.Select;
    public DataGridSelectColumn<TDto> DataGridSelectColumn => Column as DataGridSelectColumn<TDto>;
    public RenderFragment SelectRender { get; set; }
    public object SelectValue { get; set; }

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    protected override Task OnParametersSetAsync()
    {
        if (Column != null && Column.Filter != null && Column.Filter.SearchValue != null && SelectRender == null)
            SelectRender = GenerateSelectComponent();
        return base.OnParametersSetAsync();
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
    }

    private async Task Clear()
    {
        Column.Filter.SearchValue = null;
        SelectRender = GenerateSelectComponent();
        await ParentDataGrid.Refresh();
        await ParentDataGrid.Reload();
    }

    private RenderFragment GenerateSelectComponent()
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

        Console.WriteLine("Generate Select Component");
        Console.WriteLine(JsonConvert.SerializeObject(SelectItem));
        Console.WriteLine(JsonConvert.SerializeObject(((ListPageFilter)Column.Filter.SearchValue).SelectItem));

        var entityAutoComplete = typeof(EntityAutocomplete<,>).MakeGenericType(entity, entityDto);
        RenderFragment renderFragment = (builder) => { 
            builder.OpenComponent(0, entityAutoComplete);
            builder.AddAttribute(1, "SearchKey", DataGridSelectColumn.Attributes["SearchKey"]);
            if (DataGridSelectColumn.Attributes.ContainsKey("Select"))
                builder.AddAttribute(2, "CustomSelect", DataGridSelectColumn.Attributes["Select"]);
            builder.AddAttribute(3, "Data", JsonConvert.DeserializeObject(JsonConvert.SerializeObject(SelectItem), entityDto));
            builder.AddAttribute(4, "SelectedValue", JsonConvert.DeserializeObject(JsonConvert.SerializeObject(SelectItem), entityDto));

            builder.AddAttribute(5, "SelectedObjectValueChanged", async ((string Key, object Value) value) =>
            {
                SearchValue = value.Key;
                SelectItem = value.Value;
                await Filter();
            });

            builder.AddAttribute(4, "SelectedStringValueChanged", async (string value) =>
            {
                SearchValue = value;
                await Filter();
            });
            builder.CloseComponent();
        };
        return renderFragment;
    }
}
