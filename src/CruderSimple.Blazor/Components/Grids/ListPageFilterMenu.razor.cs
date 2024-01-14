using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

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

    public bool IsNumeric => Column.ColumnType == DataGridColumnType.Numeric;
    public bool IsDate => Column.ColumnType == DataGridColumnType.Date;
    public bool IsSelect => Column.ColumnType == DataGridColumnType.Select;
    public DataGridSelectColumn<TDto> DataGridSelectColumn => (DataGridSelectColumn<TDto>)Column;
    public int MyProperty { get; set; }

    protected override Task OnInitializedAsync()
    {
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
    }

    private async Task Clear()
    {
        Column.Filter.SearchValue = null;
        await ParentDataGrid.Refresh();
        await ParentDataGrid.Reload();
    }

    private RenderFragment GenerateSelectComponent(string getData, Action<string> setData)
    {
        var service = DataGridSelectColumn.Attributes["Service"];
        if (service is null)
        {
            Console.WriteLine("Service is null");
            return null;
        }

        var entity = service.GetType().GenericTypeArguments[0];
        var entityDto = service.GetType().GenericTypeArguments[1];
        // TODO: resolver get e set do Select
        var entityAutoComplete = typeof(EntityAutocomplete<,>).MakeGenericType(entity, entityDto);
        RenderFragment renderFragment = (builder) => { 
            builder.OpenComponent(0, entityAutoComplete);
            builder.AddAttribute(1, "SearchKey", DataGridSelectColumn.Attributes["SearchKey"]);
            if (DataGridSelectColumn.Attributes.ContainsKey("Select"))
                builder.AddAttribute(1, "CustomSelect", DataGridSelectColumn.Attributes["Select"]);
            builder.AddAttribute(2, "SelectedValue", getData);
            builder.AddAttribute(3, "SelectedValueChanged", new EventCallback(this, setData));
            builder.CloseComponent();
        };

        return renderFragment;
    }
}
