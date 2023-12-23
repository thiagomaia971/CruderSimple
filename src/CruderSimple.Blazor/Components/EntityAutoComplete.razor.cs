using Blazorise.Components;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CruderSimple.Blazor.Components;

[CascadingTypeParameter( nameof( TEntity ) )]
[CascadingTypeParameter( nameof( TEntityResult ) )]
[CascadingTypeParameter( nameof( TOutput ) )]
public partial class EntityAutoComplete<TEntity, TEntityResult, TOutput> : ComponentBase
    where TEntity : IEntity
    where TEntityResult : BaseDto
    where TOutput : BaseDto
{
    [Parameter]
    [EditorRequired]
    public Func<TOutput, string> TextField { get; set; }

    [Parameter]
    [EditorRequired]
    public Func<TOutput, string> ValueField { get; set; }

    [Parameter]
    [EditorRequired]
    public string SearchKey { get; set; }

    [Parameter]
    [EditorRequired]
    public Func<TEntityResult, TOutput> Convert { get; set; }

    [Parameter]
    [EditorRequired]
    public Func<TEntityResult, TOutput, bool> Compare { get; set; }

    [Parameter]
    public EventCallback<TOutput> SelectedValueChanged { get; set; }

    [Parameter]
    public EventCallback<List<TOutput>> SelectedValuesChanged { get; set; }
    
    [Parameter]
    public EventCallback<string> SelectedTextChanged { get; set; }
    
    [Parameter]
    public EventCallback<ICollection<string>> SelectedTextsChanged { get; set; }

    [Parameter]
    public TOutput SelectedValue { get; set; }
    [Parameter]
    public List<TOutput> SelectedValues { get; set; } = new List<TOutput>();
    public List<string> _selectedKeyValues { get; set; } = new List<string>();

    public List<string> SelectedKeyValues
    {
        get => _selectedKeyValues; 
        set
        {
            _selectedKeyValues = value;
            SelectedValuesChanged.InvokeAsync(SearchedOriginalData
                .Select(x => Convert(x))
                .Where(x => _selectedKeyValues.Any(y => y == ValueField(x)))
                .ToList());

        }
    }
    [Parameter]
    public string SelectedText { get; set; }
    [Parameter]
    public IEnumerable<string> SelectedTexts { get; set; }
    [Parameter]
    public bool Multiselect { get; set; }

    [Inject]
    public ICrudService<TEntity, TEntityResult> Service { get; set; }


    public IEnumerable<TEntityResult> SearchedOriginalData { get; set; } = new List<TEntityResult>();
    public IEnumerable<TOutput> SearchedData { get; set; } = new List<TOutput>();
    public Autocomplete<TOutput, string> autoComplete { get; set; }
    public AutocompleteSelectionMode SelectionMode => Multiselect ? AutocompleteSelectionMode.Multiple : AutocompleteSelectionMode.Default;
    public int TotalData { get; set; }
    public bool IsLoading { get; set; }
    public bool HasFocused { get; set; }

    protected override void OnInitialized()
    {
        InitSelectedValues();
    }

    private void InitSelectedValues()
    {
        SelectedKeyValues = SelectedValues.Select(x => ValueField(x)).ToList();
        SearchedData = SelectedValues;
        StateHasChanged();
        //SelectedValues = SearchedOriginalData
        //    .Where(x => SelectedValues.Any(y => Compare(x, y)))
        //    .Select(x => Convert(x))
        //    .ToList();
    }

    public async Task OnFocus()
    {
        HasFocused = true;
        await autoComplete.OpenDropdown();
    }

    private async Task GetData(AutocompleteReadDataEventArgs e )
    {
        if (HasFocused)
        {
            HasFocused = false;
            return;
        }

        InvokeAsync(async () =>
        {
            if (!(e?.CancellationToken.IsCancellationRequested ?? false))
            {
                IsLoading = true;
                var filter = string.IsNullOrEmpty(e?.SearchValue) ? string.Empty : $"{SearchKey} {Op.Contains} {e?.SearchValue}";

                var data = await Service.GetAll(new GetAllEndpointQuery(
                    SearchKey,
                    filter,
                    e?.VirtualizeCount ?? 0,
                    e?.VirtualizeOffset ?? 0));

                TotalData = data.Size;
                SearchedOriginalData = data.Data;
                SearchedData = SearchedOriginalData
                    .Where(x => !SelectedValues.Any(y => Compare(x, y)))
                    .Select(x => Convert(x))
                    .ToList();

                //SelectedValues = SearchedOriginalData
                //    .Where(x => SelectedValues.Any(y => Compare(x, y)))
                //    .Select(x => Convert(x))
                //    .ToList();

                IsLoading = false;
                StateHasChanged();
            }
        });
    }

    private async Task OnSelectedValueChanged(string e)
    {
        SelectedValue = SearchedData.FirstOrDefault(x => e == TextField(x));
        await SelectedValueChanged.InvokeAsync(SelectedValue);
        //StateHasChanged();
    }

    private async Task OnSelectedValuesChanged(ICollection<string> e)
    {
        SelectedValues = SearchedOriginalData
            .Select(x => Convert(x))
            .Where(x => e.Any(y => y == ValueField(x)))
            .ToList();
        SelectedKeyValues = e.ToList();

        SearchedData = SearchedOriginalData
            .Where(x => !SelectedValues.Any(y => Compare(x, y)))
            .Select(x => Convert(x))
            .ToList();
        await SelectedValuesChanged.InvokeAsync(SelectedValues);
    }

    private async Task OnSelectedTextChanged(string e)
    {
        SelectedText = e;
        await SelectedTextChanged.InvokeAsync(e);
        //StateHasChanged();
    }

    private async Task OnSelectedTextsChanged(ICollection<string> e)
    {
        SelectedTexts = e;
        await SelectedTextsChanged.InvokeAsync(e);
        //StateHasChanged();
    }

    void KeyPressHandler(KeyboardEventArgs args)
    {

       if (args.Key == "Enter")
        {
            return;
        }
        var key = (string)args.Key;
        autoComplete.Search += key;
    }
}
