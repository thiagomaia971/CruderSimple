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
public partial class MultipleEntityAutocomplete<TEntity, TEntityResult> : ComponentBase
    where TEntity : IEntity
    where TEntityResult : BaseDto
{
    [Parameter]
    public string SearchKey { get; set; }
    [Parameter]
    public List<TEntityResult> Data { get; set; }

    [Parameter]
    public EventCallback<List<TEntityResult>> SelectedValuesChanged { get; set; }

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

    public List<string> _selectedKeyValues { get; set; } = new List<string>();

    public List<string> SelectedKeyValues
    {
        get => _selectedKeyValues;
        set
        {
            _selectedKeyValues = value;
            SelectedValues = SearchedOriginalData.Where(x => value.Contains(x.GetKey)).ToList();
            SelectedValuesChanged.InvokeAsync(SelectedValues);
        }
    }

    [Inject]
    public ICrudService<TEntity, TEntityResult> Service { get; set; }


    public IEnumerable<TEntityResult> SearchedData => SearchedOriginalData.Where(x => !SelectedKeyValues.Contains(x.GetKey)).ToList();
    public IEnumerable<TEntityResult> SearchedOriginalData { get; set; } = new List<TEntityResult>();
    public Autocomplete<TEntityResult, string> autoComplete { get; set; }
    public int TotalData => SearchedData?.Count() ?? 0;
    public bool IsLoading { get; set; }

    protected override void OnParametersSet()
    {
        if (Data != null && Data.Any() && (SearchedOriginalData is null || !SearchedOriginalData.Any()))
        {
            SearchedOriginalData = Data;
            SelectedKeyValues = Data.Select(x => x.GetKey).ToList();
        }
        base.OnParametersSet();
    }

    private async Task GetData(AutocompleteReadDataEventArgs e )
    {

        InvokeAsync(async () =>
        {
            if (!(e?.CancellationToken.IsCancellationRequested ?? false))
            {
                IsLoading = true;
                StateHasChanged();
                var filter = string.IsNullOrEmpty(e?.SearchValue) ? string.Empty : $"{SearchKey} {Op.Contains} {e?.SearchValue}";

                var result = await Service.GetAll(new GetAllEndpointQuery(
                    "*",
                    filter,
                    e?.VirtualizeCount ?? 0,
                    e?.VirtualizeOffset ?? 0));

                SearchedOriginalData = result.Data
                    .ToList();

                IsLoading = false;
                StateHasChanged();
            }
        });
    }

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
public record AutocompleteDto(string Key, string Value);