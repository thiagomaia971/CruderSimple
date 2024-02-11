using Blazorise;
using Blazorise.Components;
using Blazorise.Components.Autocomplete;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Blazor.Services;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.Services;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    [Parameter] public TEntityResult SelectedValue { get; set; }
    [Parameter] public EventCallback<TEntityResult> SelectedValueChanged { get; set; }
    [Parameter] public Func<(string Key, object Value), Task> SelectedObjectValueChanged { get; set; }
    [Parameter] public RenderFragment<ItemContext<TEntityResult, string>> ItemContent { get; set; }
    [Parameter] public bool Required { get; set; } = true;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public IFluentSizing Width { get; set; }
    [Parameter] public IFluentSpacing Padding { get; set; }

    [Inject] public ICrudService<TEntity, TEntityResult> Service { get; set; }
    [Inject] public ICruderLogger<EntityAutocomplete<TEntity, TEntityResult>> Logger { get; set; }

    public bool IsFirstRender { get; set; } = true;
    public bool IgnoreSearchText { get; set; }
    public bool Focused { get; set; }

    public List<TEntityResult> SearchedData { get; set; } = new List<TEntityResult>();

    public List<TEntityResult> SearchedOriginalData { get; set; } = new List<TEntityResult>();
    public Autocomplete<TEntityResult, string> autoComplete { get; set; }
    public int TotalData { get;set;}
    public bool IsLoading { get; set; }
    public bool ShouldPrevent { get; set; }
    private bool IsSetData { get; set; } = true;
    protected override void OnParametersSet()
    {

        if (autoComplete != null && SelectedValue != null && (SearchedOriginalData is null || !SearchedOriginalData.Any()))
        {
            SearchedData = new List<TEntityResult> { SelectedValue };
            SearchedOriginalData = new List<TEntityResult> { SelectedValue };
        }
        base.OnParametersSet();
    }

    //protected override Task OnInitializedAsync()
    //{
    //    if (IsSetData)
    //    {
    //        Logger.LogDebug("Initialized: " + SelectedValue.ToJson());
    //        SearchedData = new List<TEntityResult> { SelectedValue };
    //        SearchedOriginalData = new List<TEntityResult> { SelectedValue };
    //        IsSetData = false;
    //    }
    //    return base.OnInitializedAsync();
    //}

    //protected override Task OnAfterRenderAsync(bool firstRender)
    //{
    //    if (firstRender)
    //    {
    //        Logger.LogDebug("Initialized: " + SelectedValue.ToJson());
    //        SearchedData = new List<TEntityResult> { SelectedValue };
    //        SearchedOriginalData = new List<TEntityResult> { SelectedValue };
    //    }
    //    return base.OnAfterRenderAsync(firstRender);
    //}

    private async Task SearchFocus(FocusEventArgs e)
    {
        IgnoreSearchText = true;
    }

    private async Task SearchChanged(KeyboardEventArgs e)
    {
        IgnoreSearchText = false;
    }

    private async Task GetData(AutocompleteReadDataEventArgs e )
    {
        await Logger.Watch("GetData", async () =>
        {
            if (IsFirstRender)
            {
                IsFirstRender = false;
                return;
            }

            await Search(e);
        });
    }

    private async Task Search(AutocompleteReadDataEventArgs e)
    {
        await Logger.Watch("Search", async () =>
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

            TotalData = result.Count;
            SearchedOriginalData = result.Result.ToList();
            
            var list = new List<TEntityResult>();
            //if (Data != null)
            //    list.Add(Data);
            if (SearchedOriginalData != null)
                list.AddRange(SearchedOriginalData);
            if (SelectedValue != null)
                list.AddRange(SearchedOriginalData
                    .Where(x => SelectedValue.GetKey != x.GetKey));
            SearchedData = list.DistinctBy(c => c.GetKey).ToList();
            //StateHasChanged();
        });
    }

    private string CreateSelect()
    {
        return $"{SearchKey}{(string.IsNullOrEmpty(CustomSelect) ? "" : "," + CustomSelect)}";
    }

    private string CreateFilter(AutocompleteReadDataEventArgs e)
    {
        if (IgnoreSearchText || string.IsNullOrEmpty(e?.SearchValue))
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

    public async Task ValueChanged(string values)
    {
        await Logger.Watch("ValueChanged", async () =>
        {
            SelectedValue = SearchedOriginalData.FirstOrDefault(x => values == x.GetKey);
            await SelectedValueChanged.InvokeAsync(SelectedValue);
            if (SelectedObjectValueChanged != null)
            {
                Console.WriteLine("Value changed");
                await SelectedObjectValueChanged((values, SelectedValue));
            }
        });
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
    }

    async Task KeyPressHandler(KeyboardEventArgs args)
    {

       if (args.Key == "Enter")
        {
            ShouldPrevent = true;
            return;
        }
        ShouldPrevent = false;
        await autoComplete.SearchKeyDown.InvokeAsync(args);
    }
}

public static class EntityAutocompleteUtils
{

    public static RenderFragment CreateComponent(
        Type entity, 
        Type entityDto, 
        object selectItem, 
        MulticastDelegate selectedValueChanged, 
        bool required,
        Dictionary<string, object> attributes,
        bool disabled = false)
    {
        var entityAutoComplete = typeof(EntityAutocomplete<,>).MakeGenericType(entity, entityDto);
        RenderFragment renderFragment = (builder) => {
            builder.OpenComponent(0, entityAutoComplete);
            builder.AddAttribute(1, "SearchKey", attributes["SearchKey"]);
            if (attributes.ContainsKey("Select"))
                builder.AddAttribute(2, "CustomSelect", attributes["Select"]);

            var entityDtoValue = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(selectItem), entityDto);
            //builder.AddAttribute(3, "Data", entityDtoValue);
            builder.AddAttribute(4, "SelectedValue", entityDtoValue);

            //builder.AddAttribute(5, "SelectedObjectValueChanged", selectedValueChanged);

            //builder.AddAttribute(7, "Required", required);
            //builder.AddAttribute(8, "Disabled", disabled);
            builder.CloseComponent();
        };
        return renderFragment;
    }

}