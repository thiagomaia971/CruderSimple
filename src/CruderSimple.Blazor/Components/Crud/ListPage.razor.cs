using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
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
        var dataGridSearchValues = e.Columns.Where(x => x.SearchValue != null && !string.IsNullOrEmpty((string) x.SearchValue)).ToList();
        var dataGridFields = e.Columns.Where(x => !string.IsNullOrEmpty(x.Field)).Select(x => x.Field).ToList();
        
        var select = GetQuerySelect(dataGridFields);
        var filter = GetQueryFilter(dataGridSearchValues);

        var data = await Service.GetAll(new GetAllEndpointQuery(select, filter, e.PageSize, e.Page));

        TotalData = data.Size;
        Data = data.Data;
        await DataGridRef.Refresh();

        IsLoading = false;
        StateHasChanged();
    }

    private string GetQuerySelect(List<string> dataGridFields)
    {
        var select = string.Join(",", dataGridFields);
        
        if (!string.IsNullOrEmpty(CustomSelect))
            select += $",{CustomSelect}";

        return select;
    }

    private string GetQueryFilter(List<DataGridColumnInfo> dataGridColumnInfos)
    {
        var filters = new List<string>();
        foreach (var columnInfo in dataGridColumnInfos)
        {
            if (string.IsNullOrEmpty((string) columnInfo.SearchValue))
                continue;
            if (columnInfo.FilterMethod is null)
                filters.Add($"{columnInfo.Field} {DataGridColumnFilterMethod.Contains} {columnInfo.SearchValue}");
            else
                filters.Add($"{columnInfo.Field} {columnInfo.FilterMethod.Value} {columnInfo.SearchValue}");
        }
        return string.Join(",", filters);
    }

    protected Task GoBack() => PageHistorysState.GoBack();

}