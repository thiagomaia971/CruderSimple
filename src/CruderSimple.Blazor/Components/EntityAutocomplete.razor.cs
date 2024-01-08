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
public partial class EntityAutocomplete<TEntity, TEntityResult> : ComponentBase
    where TEntity : IEntity
    where TEntityResult : BaseDto
{
    [Parameter]
    public string SearchKey { get; set; }
    [Parameter]
    public string CustomSelect { get; set; }
    [Parameter]
    public TEntityResult Data { get; set; }

    [Parameter]
    public EventCallback<TEntityResult> SelectedValueChanged { get; set; }

    [Parameter]
    public RenderFragment<ItemContext<TEntityResult, string>> ItemContent { get; set; }

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

    public IEnumerable<TEntityResult> SearchedOriginalData { get; set; } = new List<TEntityResult>();
    public Autocomplete<TEntityResult, string> autoComplete { get; set; }
    public int TotalData => SearchedData?.Count() ?? 0;
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
                    e?.VirtualizeCount ?? 0,
                    e?.VirtualizeOffset ?? 0));

                SearchedOriginalData = result.Data
                    .ToList();

                StateHasChanged();
            }
        });
    }

    public void ValueChanged(string values)
    {
        SelectedValue = SearchedOriginalData.FirstOrDefault(x => values == x.GetKey);
        SelectedValueChanged.InvokeAsync(SelectedValue);
    }

    async Task sIsValidValue(ValidatorEventArgs e, CancellationToken c)
    {
        e.Status = SelectedValue is not null ? ValidationStatus.Success : ValidationStatus.Error;

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