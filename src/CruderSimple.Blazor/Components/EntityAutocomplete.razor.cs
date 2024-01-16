using Blazorise;
using Blazorise.Components;
using Blazorise.Components.Autocomplete;
using Blazorise.DataGrid;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.Services;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace CruderSimple.Blazor.Components;

[CascadingTypeParameter( nameof( TEntity ) )]
[CascadingTypeParameter( nameof( TEntityResult ) )]
public partial class EntityAutocomplete<TEntity, TEntityResult> : ComponentBase
    where TEntity : IEntity
    where TEntityResult : BaseDto
{
    [Parameter] public string SearchKey { get; set; }
    [Parameter] public string CustomSelect { get; set; }
    [Parameter] public string OrderBy { get; set; }
    [Parameter] public TEntityResult Data { get; set; }
    [Parameter] public EventCallback<TEntityResult> SelectedValueChanged { get; set; }
    [Parameter] public Func<string, Task> SelectedStringValueChanged { get; set; }
    [Parameter] public Func<(string Key, object Value), Task> SelectedObjectValueChanged { get; set; }
    [Parameter] public RenderFragment<ItemContext<TEntityResult, string>> ItemContent { get; set; }
    [Parameter] public bool Required { get; set; } = true;

    private TEntityResult _selectedValue { get; set; }

    [Parameter]
    public TEntityResult SelectedValue 
    { 
        get => _selectedValue; 
        set 
        {
            _selectedValue = value;
        }
    }

    [Parameter]
    public bool Disabled { get; set; } = false;
    
    [Parameter] 
    public IFluentSizing Width { get; set; }
    
    [Parameter] 
    public IFluentSpacing Padding { get; set; }

    [Inject]
    public ICrudService<TEntity, TEntityResult> Service { get; set; }

    [Inject]
    public PermissionService PermissionService { get; set; }

    [Inject]
    public DebounceService DebounceService { get; set; }

    public bool IsFirstRender { get; set; } = true;
    public bool Focused { get; set; }


    public IEnumerable<TEntityResult> SearchedData
    {
        get
        {
            var list = new List<TEntityResult>();
            if (Data is not null)
                list.Add(Data);
            if (SearchedOriginalData is not null)
                list.AddRange(SearchedOriginalData);
            if (SelectedValue is not null) 
                list.AddRange(SearchedOriginalData
                    .Where(x => SelectedValue.GetKey != x.GetKey));
            return list.DistinctBy(c => c.GetKey).ToList();
        }
    }

    public List<TEntityResult> SearchedOriginalData { get; set; } = new List<TEntityResult>();
    public Autocomplete<TEntityResult, string> autoComplete { get; set; }
    public int TotalData { get;set;}
    public bool IsLoading { get; set; }
    public bool ShouldPrevent { get; set; }

    protected override void OnParametersSet()
    {
        if (Data != null && (SearchedOriginalData is null || !SearchedOriginalData.Any()))
        {
            SearchedOriginalData = new List<TEntityResult>{Data};
            SelectedValue = Data;
        }
        base.OnParametersSet();
    }

    private async Task SearchFocus(FocusEventArgs e)
    {
        Focused = true;
    }

    private async Task SearchChanged(KeyboardEventArgs e)
    {
        Focused = false;
    }

    private async Task GetData(AutocompleteReadDataEventArgs e )
    {
            if (IsFirstRender)
            {
                IsFirstRender = false;
                return;
            }

            await Search(e);
    }

    private async Task Search(AutocompleteReadDataEventArgs e)
    {
        var select = CreateSelect();
        var filter = CreateFilter(e);
        var orderBy = CreateOrderBy();

        var result = await Service.GetAll(new GetAllEndpointQuery(
                select,
                filter,
                orderBy,
                e.VirtualizeCount,
                0,
                e.VirtualizeOffset));

        TotalData = result.Size;
        SearchedOriginalData = result.Data.ToList();
        StateHasChanged();
    }

    private string CreateSelect()
    {
        return $"{SearchKey}{(string.IsNullOrEmpty(CustomSelect) ? "" : "," + CustomSelect)}";
    }

    private string CreateFilter(AutocompleteReadDataEventArgs e)
    {
        if (Focused || string.IsNullOrEmpty(e?.SearchValue))
            return string.Empty;
        
        var filter = new List<string>();
        var searchKeys = SearchKey.Split(",");
        foreach ( var key in searchKeys )
            filter.Add($"{key} {Op.Contains} {e?.SearchValue}");
        string result = string.Join(" OR ", filter);

        return result;
    }

    private string CreateOrderBy()
    {
        if (string.IsNullOrEmpty(OrderBy))
            return $"{SearchKey.Split(",")[0]} {SortDirection.Ascending}";
        return $"{OrderBy} {SortDirection.Ascending}";
    }

    public void ValueChanged(string values)
    {
        SelectedValue = SearchedOriginalData.FirstOrDefault(x => values == x.GetKey);
        SelectedValueChanged.InvokeAsync(SelectedValue);
        if (SelectedStringValueChanged is not null)
            SelectedStringValueChanged(values);
        if (SelectedObjectValueChanged is not null)
            SelectedObjectValueChanged((values, SelectedValue));
    }

    async Task sIsValidValue(ValidatorEventArgs e, CancellationToken c)
    {
        if (Required)
        {
            e.Status = SelectedValue is not null ? ValidationStatus.Success : ValidationStatus.Error;

            if (e.Status == ValidationStatus.Error)
                e.ErrorText = "Selecione pelo menos um";
            else
                e.ErrorText = "OK";
        }
        else
        {
            e.Status = ValidationStatus.Success;
            e.ErrorText = "OK";
        }
    }

    async Task KeyPressHandler(KeyboardEventArgs args)
    {

       if (args.Key == "Enter")
        {
            ShouldPrevent = true;
            return;
        }
        ShouldPrevent = false;
        var key = (string)args.Key;
        await autoComplete.SearchKeyDown.InvokeAsync(args);
    }
}

public static class EntityAutocompleteUtils
{

    public static RenderFragment CreateComponent(Type entity, Type entityDto, object selectItem, MulticastDelegate selectedValueChanged, bool required, Dictionary<string, object> attributes)
    {
        var entityAutoComplete = typeof(EntityAutocomplete<,>).MakeGenericType(entity, entityDto);
        RenderFragment renderFragment = (builder) => {
            builder.OpenComponent(0, entityAutoComplete);
            builder.AddAttribute(1, "SearchKey", attributes["SearchKey"]);
            if (attributes.ContainsKey("Select"))
                builder.AddAttribute(2, "CustomSelect", attributes["Select"]);

            if (selectItem is not null)
            {

                builder.AddAttribute(3, "Data", JsonConvert.DeserializeObject(JsonConvert.SerializeObject(selectItem), entityDto));
                builder.AddAttribute(4, "SelectedValue", JsonConvert.DeserializeObject(JsonConvert.SerializeObject(selectItem), entityDto));
            }

            builder.AddAttribute(5, "SelectedObjectValueChanged", selectedValueChanged);

            builder.AddAttribute(7, "Required", required);
            builder.CloseComponent();
        };
        return renderFragment;
    }

}