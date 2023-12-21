using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Timers;
using Blazorise.DataGrid;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Blazor.Services;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Crud;

[CascadingTypeParameter( nameof( TEntity ) )]
[CascadingTypeParameter( nameof( TDto ) )]
public partial class ListPage<TEntity, TDto> : ComponentBase
    where TEntity : IEntity
    where TDto : BaseDto
{
    [Parameter] public RenderFragment Columns { get; set; }
    [Parameter] public RenderFragment Modal { get; set; }
    [Parameter] public string CustomSelect { get; set; }

    [Inject] 
    public PermissionService PermissionService { get; set; }

    [Inject]
    public ICrudService<TEntity, TDto> Service { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Inject]
    public PageHistoryState PageHistorysState { get; set; }

    [Inject]
    public DebounceService DebounceService { get; set; }

    public IEnumerable<TDto> Data { get; set; }
    public int TotalData { get; set; }
    public int PageSize { get; set; }
    public DataGrid<TDto> DataGridRef { get; set; }
    public bool IsLoading { get; set; }
    public string NewPage => $"{typeof(TEntity).Name}/";

    private async Task GetData(DataGridReadDataEventArgs<TDto> e)
    {
        if ( !e.CancellationToken.IsCancellationRequested )
        {
            if (!IsLoading)
            {
                DebounceService.Start(300, Search, e);
            }
        }
    }

    private async Task Search(DataGridReadDataEventArgs<TDto> e)
    {
        if (IsLoading) 
            return;

        IsLoading = true;
        StateHasChanged();
        var dataGridColumnInfos = e.Columns.Where(x => x.SearchValue != null && !string.IsNullOrEmpty((string) x.SearchValue)).ToList();
        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        foreach (var columnInfo in dataGridColumnInfos)
        {
            if (columnInfo.FilterMethod is null)
            {
                queryString.Add(columnInfo.Field, $"contains {columnInfo.SearchValue}");
                continue;
            }
            switch (columnInfo.FilterMethod.Value)
            {
                case DataGridColumnFilterMethod.StartsWith:
                    queryString.Add(columnInfo.Field, $"startWith {columnInfo.SearchValue}");
                    break;
                case DataGridColumnFilterMethod.Contains:
                    queryString.Add(columnInfo.Field, $"contains {columnInfo.SearchValue}");
                    break;
                case DataGridColumnFilterMethod.EndsWith:
                    queryString.Add(columnInfo.Field, $"endWith {columnInfo.SearchValue}");
                    break;
                case DataGridColumnFilterMethod.Equals:
                    queryString.Add(columnInfo.Field, $"eq {columnInfo.SearchValue}");
                    break;
                case DataGridColumnFilterMethod.NotEquals:
                    queryString.Add(columnInfo.Field, $"neq {columnInfo.SearchValue}");
                    break;
                case DataGridColumnFilterMethod.LessThan:
                    queryString.Add(columnInfo.Field, $"lt {columnInfo.SearchValue}");
                    break;
                case DataGridColumnFilterMethod.LessThanOrEqual:
                    queryString.Add(columnInfo.Field, $"lteq {columnInfo.SearchValue}");
                    break;
                case DataGridColumnFilterMethod.GreaterThan:
                    queryString.Add(columnInfo.Field, $"gt {columnInfo.SearchValue}");
                    break;
                case DataGridColumnFilterMethod.GreaterThanOrEqual:
                    queryString.Add(columnInfo.Field, $"gteq {columnInfo.SearchValue}");
                    break;
            }
        }

        var select = string.Join(",", e.Columns.Where(x => !string.IsNullOrEmpty(x.Field)).Select(x => x.Field));
        var xx = e.Columns.ToList();
        if (!string.IsNullOrEmpty(CustomSelect))
            select += $",{CustomSelect}";
        var data = await Service.GetAll(new GetAllEndpointQuery(select, e.PageSize, e.Page), queryString.ToString());

        TotalData = data.Size;
        Data = data.Data;
        await DataGridRef.Refresh();

        IsLoading = false;
        StateHasChanged();
    }

    protected Task GoBack() => PageHistorysState.GoBack();

}