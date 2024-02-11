using Blazorise;
using Blazorise.Components;
using Blazorise.Components.Autocomplete;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.Services;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CruderSimple.Blazor.Components;

[CascadingTypeParameter( nameof( TEntity ) )]
[CascadingTypeParameter( nameof( TEntityResult ) )]
public partial class MultipleEntityAutocomplete<TEntity, TEntityResult> : ComponentBase
    where TEntity : IEntity
    where TEntityResult : BaseDto
{
    [Parameter]
    public string SearchKey { get; set; }
    [Parameter]
    public string CustomSelect { get; set; }
    [Parameter]
    public List<TEntityResult> Data { get; set; }

    [Parameter]
    public EventCallback<List<TEntityResult>> SelectedValuesChanged { get; set; }

    [Parameter]
    public RenderFragment<ItemContext<TEntityResult, string>> ItemContent { get; set; }
    
    [Parameter] 
    public IFluentSizing Width { get; set; }
    
    [Parameter] 
    public IFluentSpacing Padding { get; set; }

    private List<TEntityResult> _selectedValues { get; set; } = new List<TEntityResult>();

    [Parameter]
    public List<TEntityResult> SelectedValues 
    { 
        get => _selectedValues; 
        set 
        {
            _selectedValues = value;
        }
    }

    [Parameter]
    public bool Disabled { get; set; } = false;

    [Inject]
    public ICrudService<TEntity, TEntityResult> Service { get; set; }

    [Inject]
    public PermissionService PermissionService { get; set; }


    public IEnumerable<TEntityResult> SearchedData
    {
        get
        {
            var list = new List<TEntityResult>();
            if (Data is not null)
                list.AddRange(Data);
            if (SearchedOriginalData is not null) 
                list.AddRange(SearchedOriginalData
                    .Where(x => !(SelectedValues?.Any(y => y.GetKey == x.GetKey) ?? false)));
            return list.Distinct().ToList();
        }
    }

    public IEnumerable<TEntityResult> SearchedOriginalData { get; set; } = new List<TEntityResult>();
    public Autocomplete<TEntityResult, string> autoComplete { get; set; }
    public int TotalData => SearchedData?.Count() ?? 0;
    public bool IsLoading { get; set; }
    public bool ShouldPrevent { get; set; }

    protected override void OnParametersSet()
    {
        if (Data != null && Data.Any() && (SearchedOriginalData is null || !SearchedOriginalData.Any()))
        {
            SearchedOriginalData = Data;
            SelectedValues = Data;
        }
        base.OnParametersSet();
    }

    private async Task GetData(AutocompleteReadDataEventArgs e )
    {

        InvokeAsync(async () =>
        {
            if (!(e?.CancellationToken.IsCancellationRequested ?? false))
            {
                var select = $"{SearchKey}{(string.IsNullOrEmpty(CustomSelect) ? "" : ","+CustomSelect)}";
                var filter = string.IsNullOrEmpty(e?.SearchValue) ? string.Empty : $"{SearchKey} {Op.Contains} {e?.SearchValue}";
                var orderBy = $"{SearchKey} {SortDirection.Ascending}";

                var result = await Service.GetAll(new GetAllEndpointQuery(
                    select,
                    filter,
                    orderBy,
                    e.VirtualizeCount,
                    0,
                    e.VirtualizeOffset));

                SearchedOriginalData = result.Result
                    .ToList();

                StateHasChanged();
            }
        });
    }

    public void ValuesChanged(IEnumerable<string> values)
    {
        SelectedValues = SearchedOriginalData.Where(x => values.Contains(x.GetKey)).ToList();
        SelectedValuesChanged.InvokeAsync(SelectedValues);
    }

    async Task sIsValidValue(ValidatorEventArgs e, CancellationToken c)
    {
        e.Status = SelectedValues.Any() ? ValidationStatus.Success : ValidationStatus.Error;

        if (e.Status == ValidationStatus.Error)
            e.ErrorText = "Selecione pelo menos um";
        else
            e.ErrorText = "OK";
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