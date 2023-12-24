using Blazorise;
using Blazorise.Components;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

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
    public Func<TEntityResult, string> TextField { get; set; }
    public Expression<Func<TEntityResult, string>> TextFieldExpression => x => TextField(x);

    [Parameter]
    [EditorRequired]
    public Func<TOutput, string> KeyField { get; set; }

    [Parameter]
    [EditorRequired]
    public Func<TOutput, TEntityResult> ToEntity { get; set; }

    [Parameter]
    [EditorRequired]
    public string SearchKey { get; set; }

    [Parameter]
    [EditorRequired]
    public Func<TEntityResult, TOutput> Convert { get; set; }

    [Parameter]
    [EditorRequired]
    public Func<TEntityResult, TOutput, bool> Compare { get; set; }

    private List<TOutput> _selectedValues { get; set; } = new List<TOutput>();

    [Parameter]
    public List<TOutput> SelectedValues 
    { 
        get => _selectedValues; 
        set 
        {
            if (value != null && _selectedValues != value)
            {
                _selectedValues = value;
                SelectedValuesChanged.InvokeAsync(value);
                SelectedKeyValues = value.Select(x => KeyField(x)).ToList();
                if (SearchedData is null || !SearchedData.Any())
                    SearchedOriginalData = value.Select(x => ToEntity(x)).ToList();
                StateHasChanged();
            }
        }
    }

    private List<string> _selectedKeyValues { get; set; } = new List<string>();

    public List<string> SelectedKeyValues
    {
        get => _selectedKeyValues; 
        set
        {
            if (value != null && (_selectedKeyValues.Any(x => !value.Contains(x)) || 
                value.Any(x => !_selectedKeyValues.Contains(x))))
            {

                _selectedKeyValues = value;
                var fromPassedValue = SelectedValues
                    .Where(x => value.Contains(KeyField(x)))
                    .ToList();
                var fromExternalData = SearchedData
                    .Where(x => value.Any(y => x.Id == y))
                    .Select(x => Convert(x))
                    .ToList();
                SelectedValues = fromPassedValue.Concat(fromExternalData).DistinctBy(x => KeyField(x)).ToList();
                StateHasChanged();
                //_selectedValues = SearchedData?
                //    .Select(x => Convert(x))
                //    .Where(x => _selectedKeyValues.Any(y => y == ValueField(x)))
                //    .ToList();
            }
        }
    }

    [Parameter]
    public EventCallback<List<TOutput>> SelectedValuesChanged { get; set; }

    [Parameter]
    public bool Multiselect { get; set; }

    [Inject]
    public ICrudService<TEntity, TEntityResult> Service { get; set; }


    public IEnumerable<TEntityResult> SearchedData => SearchedOriginalData?/*.Where(x => !SelectedKeyValues.Any(y => x.Id == y))*/.ToList() ?? new List<TEntityResult>();
    public IEnumerable<TEntityResult> SearchedOriginalData { get; set; } = new List<TEntityResult>();
    public Autocomplete<TEntityResult, string> autoComplete { get; set; }
    public AutocompleteSelectionMode SelectionMode => Multiselect ? AutocompleteSelectionMode.Multiple : AutocompleteSelectionMode.Default;
    public int TotalData => SearchedData?.Count() ?? 0;
    public bool IsLoading { get; set; }
    public bool HasFocused { get; set; }
    public bool IsFirstTime { get; set; } = true;

    private void InitSelectedValues()
    {
        //SelectedKeyValues = SelectedValues?.Select(x => ValueField(x)).ToList();
        //if (SearchedData is null || !SearchedData.Any())
        //    SearchedData = SelectedValues;
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

                var result = await Service.GetAll(new GetAllEndpointQuery(
                    SearchKey,
                    filter,
                    e?.VirtualizeCount ?? 0,
                    e?.VirtualizeOffset ?? 0));

                SearchedOriginalData = SelectedValues
                    .Select(x => ToEntity(x))
                    .Concat(result.Data.Where(x => !SelectedValues.Any(y => Compare(x, y))))
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

    //private async Task OnSelectedValueChanged(string e)
    //{
    //    SelectedValue = SearchedData.FirstOrDefault(x => e == TextField(x));
    //    await SelectedValueChanged.InvokeAsync(SelectedValue);
    //    //StateHasChanged();
    //}

    //private async Task OnSelectedValuesChanged(ICollection<string> e)
    //{
    //    SelectedValues = SearchedOriginalData
    //        .Select(x => Convert(x))
    //        .Where(x => e.Any(y => y == ValueField(x)))
    //        .ToList();
    //    SelectedKeyValues = e.ToList();

    //    SearchedData = SearchedOriginalData
    //        .Where(x => !SelectedValues.Any(y => Compare(x, y)))
    //        .Select(x => Convert(x))
    //        .ToList();
    //    await SelectedValuesChanged.InvokeAsync(SelectedValues);
    //}

    //private async Task OnSelectedTextChanged(string e)
    //{
    //    SelectedText = e;
    //    await SelectedTextChanged.InvokeAsync(e);
    //    //StateHasChanged();
    //}

    //private async Task OnSelectedTextsChanged(ICollection<string> e)
    //{
    //    SelectedTexts = e;
    //    await SelectedTextsChanged.InvokeAsync(e);
    //    //StateHasChanged();
    //}


    void IsValidValue(ValidatorEventArgs e)
    {
        Console.WriteLine(e.Value);
        e.Status = SelectedKeyValues.Any() ? ValidationStatus.Success : ValidationStatus.Error;

        if (e.Status == ValidationStatus.Error)
        {
            e.ErrorText = "ERROR";
        }
        else
        {
            e.ErrorText = "OK";
        }
    }
    async Task sIsValidValue(ValidatorEventArgs e, CancellationToken c)
    {
        Console.WriteLine(e.Value);
        e.Status = SelectedKeyValues.Any() ? ValidationStatus.Success : ValidationStatus.Error;

        if (e.Status == ValidationStatus.Error)
        {
            e.ErrorText = "ERROR";
        }
        else
        {
            e.ErrorText = "OK";
        }
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
