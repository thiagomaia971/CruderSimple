using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using Blazorise.DataGrid;
using Blazorise.LoadingIndicator;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Blazor.Services;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
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

    [CascadingParameter]
    public LoadingIndicator Loading { get; set; }

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
    public string NewPage => $"{typeof(TEntity).Name}/";

    private async Task GetData(DataGridReadDataEventArgs<TDto> e)
    {
        if ( !e.CancellationToken.IsCancellationRequested )
        {
            await Loading.Show();
            StateHasChanged();
            DebounceService.Start(300, Search, e);
        }
    }

    private async Task Search(DataGridReadDataEventArgs<TDto> e)
    {
        var xx = e.Columns.ToList();
        InvokeAsync(async () =>
        {
            var select = GetQuerySelect(e.Columns);
            var filter = GetQueryFilter(e.Columns);
            var orderByColumn = e.Columns.FirstOrDefault(x => x.SortIndex == 0);
            var orderBy = orderByColumn is null ? null : $"{orderByColumn.SortField} {orderByColumn.SortDirection}";

            var data = await Service.GetAll(new GetAllEndpointQuery(select, filter, orderBy, e.PageSize, e.Page));

            TotalData = data.Size;
            Data = data.Data;
            await DataGridRef.Refresh();

            await Loading.Hide();
            StateHasChanged();
        });
    }

    private string GetQuerySelect(IEnumerable<DataGridColumnInfo> dataGridFields)
    {
        var select = string.Join(",", dataGridFields
            .Where(x => !string.IsNullOrEmpty(x.Field))
            .Select(x =>
            {
                if (typeof(TDto).IsPropertyEnumerableType(x.Field))
                    return $"{x.Field}.Id";
                return x.Field;
            }));
        
        if (!string.IsNullOrEmpty(CustomSelect))
            select += $",{CustomSelect}";

        return select;
    }

    private string GetQueryFilter(IEnumerable<DataGridColumnInfo> dataGridColumnInfos)
    {
        var filters = new List<string>();
        foreach (var columnInfo in dataGridColumnInfos.Where(x =>
                    x.SearchValue != null &&
                    !string.IsNullOrEmpty((string)x.SearchValue) &&
                    !string.IsNullOrEmpty(x.Field)))
        {
            if (string.IsNullOrEmpty((string) columnInfo.SearchValue))
                continue;
            if (typeof(TDto).IsPropertyEnumerableType(columnInfo.Field))
                filters.Add($"{columnInfo.Field}.Id {Op.AnyEquals} {columnInfo.SearchValue}");
            else if (columnInfo.FilterMethod is null)
                filters.Add($"{columnInfo.Field} {DataGridColumnFilterMethod.Contains} {columnInfo.SearchValue}");
            else
                filters.Add($"{columnInfo.Field} {columnInfo.FilterMethod.Value} {columnInfo.SearchValue}");
        }
        return string.Join(",", filters);
    }

    protected Task GoBack() => PageHistorysState.GoBack();

}