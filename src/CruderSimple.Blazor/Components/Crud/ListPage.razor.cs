using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Timers;
using Blazored.LocalStorage;
using Blazorise;
using Blazorise.DataGrid;
using Blazorise.LoadingIndicator;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Blazor.Services;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.Services;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

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

    [Inject]
    public DimensionService DimensionService { get; set; }
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] private ILocalStorageService LocalStorage { get; set;}

    public IEnumerable<TDto> Data { get; set; }
    public int TotalData { get; set; }
    public DataGrid<TDto> DataGridRef { get; set; }
    public string NewPage => $"{typeof(TEntity).Name}/";
    public ViewEditDeleteServiceButtons<TEntity, TDto> ViewEditDeleteButtons { get; set;}
    public bool IsFirstRender { get; set; } = true;


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Loading.Show();
            await SearchSelects();
            await Loading.Hide();
        }
    }

    private async Task GetData(DataGridReadDataEventArgs<TDto> e)
    {
        if (!e.CancellationToken.IsCancellationRequested)
        {
            await Loading.Show();
            StateHasChanged();

            if (IsFirstRender)
            {
                IsFirstRender = false;
                await LoadColumns();
                StateHasChanged();
            }

            await Search(e);

            await Loading.Hide();
            StateHasChanged();
        }
    }

    private async Task SearchSelects()
    {
        var selects = DataGridRef.GetColumns().Where(x => x.ColumnType == DataGridColumnType.Select).ToList();
        foreach (var select in selects)
        {
            var selectColumn = (DataGridSelectColumn<TDto>) select;
            var field = (string)selectColumn.Attributes["Field"];
            var attributeService = (dynamic) selectColumn.Attributes["Service"];
            var defaultValue = (object) selectColumn.Attributes["Default"];
            if (string.IsNullOrEmpty(field) || attributeService is null)
                return;

            var result = await attributeService.GetAll(new GetAllEndpointQuery(field, null, null, 0, 0));
            var list = new List<object> { defaultValue };
            list.AddRange((IEnumerable<object>)result.Data);
            selectColumn.Data = list;
        }
    }

    private async Task Search(DataGridReadDataEventArgs<TDto> e)
    {
        var select = GetQuerySelect(e.Columns);
        var filter = GetQueryFilter(e.Columns);
        var xx = e.Columns.ToList();
        var xxx = e.Columns.Select(x => x.SortIndex).ToList();
        var orderByColumn = e.Columns.FirstOrDefault(x => x.SortIndex == 0);
        var orderBy = orderByColumn is null ? null : $"{orderByColumn.SortField} {orderByColumn.SortDirection}";

        var data = await Service.GetAll(new GetAllEndpointQuery(select, filter, orderBy, e.PageSize, e.Page));

        TotalData = data.Size;
        Data = data.Data;
        //await DataGridRef.Refresh();
        await SaveColumns();
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
                    !string.IsNullOrEmpty(x.Field)))
        {
            var filter = (ListPageFilter)columnInfo.SearchValue;
            if (string.IsNullOrEmpty(filter.SearchValue))
                continue;
            var searchValues = filter.SearchValue.Split("_");

            switch (columnInfo.ColumnType)
            {
                case DataGridColumnType.Text:
                case DataGridColumnType.Numeric:
                case DataGridColumnType.Check:
                    filters.Add($"{columnInfo.Field} {filter.FilterMethod} {searchValues[0]}");
                    break;
                case DataGridColumnType.Date:
                    if (!string.IsNullOrEmpty(searchValues[0]))
                        filters.Add($"{columnInfo.Field} {Op.GreaterThanOrEqual} {searchValues[0]}");
                    if (!string.IsNullOrEmpty(searchValues[1]))
                        filters.Add($"{columnInfo.Field} {Op.LessThanOrEqual} {searchValues[1]}");
                    break;
                case DataGridColumnType.MultiSelect:
                case DataGridColumnType.Select:
                    // TODO: implementar AnyContains e MultiSelect

                    var selectColumn = (DataGridSelectColumn<TDto>) DataGridRef.GetColumns().FirstOrDefault(x => x.Field == columnInfo.Field);
                    var field = (string)selectColumn.Attributes["KeySearch"];
                    if (field != null)
                        filters.Add($"{field} {Op.AnyEquals} {searchValues[0]}");
                    else
                        filters.Add($"{columnInfo.Field}.Id {Op.AnyEquals} {searchValues[0]}");
                    break;
                case DataGridColumnType.Command:
                    break;
            }
        }
        return string.Join(",", filters);
    }

    private async Task LoadColumns()
    {
        var data = await LocalStorage.GetItemAsync<ListPageLocalStorage<TDto>>($"ListPage:{typeof(TEntity).Name}.{typeof(TDto).Name}");
        var columns = DataGridRef.GetColumns();

        if (data is not null)
        {
            DataGridRef.CurrentPage = data.CurrentPage;
            DataGridRef.PageSize = data.PageSize;

            foreach (var column in columns)
            {
                var columnSaved = data.Columns.FirstOrDefault(x => x.Caption == column.Caption && x.Field == column.Field);
                if (columnSaved is null)
                    continue;

                column.Filter.SearchValue = columnSaved.Filter;
                column.SortDirection = columnSaved.CurrentSortDirection;
                column.SortOrder = columnSaved.SortOrder;
                column.SortField = column.Field;
                column.Width = string.IsNullOrEmpty(columnSaved.Width) ? column.Width : columnSaved.Width;
            }
        }

        var columnToSort = columns.FirstOrDefault(x => x.SortDirection != SortDirection.Default);
        if (columnToSort is null)
            await DataGridRef.Refresh();
        else
            await DataGridRef.Sort(
                string.IsNullOrEmpty(columnToSort.SortField) ? columnToSort.Field : columnToSort.SortField,
                columnToSort.SortDirection);

        await DataGridRef.Refresh();
    }

    private async Task SaveColumns()
    {
        var columns = DataGridRef.GetColumns().Select(x => new ListPageColumnLocalStorage<TDto>(
            x.Caption, 
            x.Field, 
            x.Filter?.SearchValue as ListPageFilter,
            x.CurrentSortDirection,
            x.SortOrder,
            x.Width)
        ).ToList();
        var data = new ListPageLocalStorage<TDto>(DataGridRef.CurrentPage, DataGridRef.PageSize, columns);
        await LocalStorage.SetItemAsync($"ListPage:{typeof(TEntity).Name}.{typeof(TDto).Name}", data);
    }

    protected Task GoBack() => PageHistorysState.GoBack();

    public async Task SingleClicked(DataGridRowMouseEventArgs<TDto> e)
    {
        ViewEditDeleteButtons.Item = e.Item;
        if (PermissionService.CanWrite)
            ViewEditDeleteButtons.ToEdit(e.MouseEventArgs.CtrlKey);
        else 
            ViewEditDeleteButtons.ToView(e.MouseEventArgs.CtrlKey);
    }

    public async Task CopyToClipboard(DataGridRowMouseEventArgs<TDto> e)
    {
        ViewEditDeleteButtons.Item = e.Item;
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", "asdas");
    }
}

public record ListPageLocalStorage<TDto>(
    int CurrentPage,
    int PageSize,
    List<ListPageColumnLocalStorage<TDto>> Columns)
    where TDto : BaseDto;

public record ListPageColumnLocalStorage<TDto>(
    string Caption,
    string Field,
    ListPageFilter Filter,
    SortDirection CurrentSortDirection,
    int SortOrder,
    string Width)
    where TDto : BaseDto;

public class ListPageFilter
{
    public string SearchValue { get; set; }
    public DataGridColumnFilterMethod FilterMethod { get; set; }
}