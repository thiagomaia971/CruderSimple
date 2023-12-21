using Blazorise.Components;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components;

[CascadingTypeParameter( nameof( TEntity ) )]
[CascadingTypeParameter( nameof( TDto ) )]
public partial class EntityAutoComplete<TEntity, TDto> : ComponentBase
    where TEntity : IEntity
    where TDto : BaseDto
{
    [EditorRequired]
    [Parameter]
    public Func<TDto, string> TextField { get; set; }

    [EditorRequired]
    [Parameter]
    public Func<TDto, string> ValueField { get; set; }

    [Parameter]
    public string Select { get; set; }

    [Parameter]
    public string Filter { get; set; }

    [Parameter]
    public EventCallback<string> SelectedValueChanged { get; set; }
    
    [Parameter]
    public EventCallback<string> SelectedTextChanged { get; set; }


    [Inject]
    public ICrudService<TEntity, TDto> Service { get; set; }


    [Parameter]
    public string SelectedValue { get; set; }
    [Parameter]
    public string SelectedText { get; set; }

    public IEnumerable<TDto> Data { get; set; }
    public Autocomplete<TDto, string> autoComplete { get; set; }
    public int TotalData { get; set; }
    public bool IsLoading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // var user = await state.GetUserInfo();
        // Routes = user.Routes;
    }

    public async Task OnBlur()
    {
        await autoComplete.OpenDropdown();
    }

    private async Task GetData(AutocompleteReadDataEventArgs e )
    {
        if ( !e.CancellationToken.IsCancellationRequested )
        {
            IsLoading = true;
            await Task.Delay(100);
            
            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryString.Add(Filter, e.SearchValue);
            var data = await Service.GetAll(new GetAllEndpointQuery(Select, e.VirtualizeCount, e.VirtualizeOffset), queryString.ToString());

            if ( !e.CancellationToken.IsCancellationRequested )
            {
                TotalData = data.Size;
                Data = data.Data;
            }
            IsLoading = false;
        }
    }

    private async Task OnSelectedValueChanged(string e)
    {
        SelectedValue = e;
        await SelectedValueChanged.InvokeAsync(e);
    }

    private async Task OnSelectedTextChanged(string e)
    {
        SelectedText = e;
        await SelectedTextChanged.InvokeAsync(e);
    }
}
