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
using CruderSimple.Core.Services;
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

    [Inject]
    public DimensionService DimensionService { get; set; }

    public IEnumerable<TDto> Data { get; set; }
    public int TotalData { get; set; }
    public int PageSize { get; set; }
    public DataGrid<TDto> DataGridRef { get; set; }
    public string NewPage => $"{typeof(TEntity).Name}/";
    public ViewEditDeleteServiceButtons<TEntity, TDto> ViewEditDeleteButtons { get; set;}


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await SearchSelects();
    }

    private async Task GetData(DataGridReadDataEventArgs<TDto> e)
    {
        if ( !e.CancellationToken.IsCancellationRequested )
        {
            await Loading.Show();
            StateHasChanged();
            await Search(e);
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
        var orderByColumn = e.Columns.FirstOrDefault(x => x.SortIndex == 0);
        var orderBy = orderByColumn is null ? null : $"{orderByColumn.SortField} {orderByColumn.SortDirection}";

        var data = await Service.GetAll(new GetAllEndpointQuery(select, filter, orderBy, e.PageSize, e.Page));

        TotalData = data.Size;
        Data = data.Data;
        await DataGridRef.Refresh();

        await Loading.Hide();
        StateHasChanged();
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

            switch (columnInfo.ColumnType)
            {
                case DataGridColumnType.Text:
                case DataGridColumnType.Numeric:
                case DataGridColumnType.Check:
                    filters.Add($"{columnInfo.Field} {(columnInfo.FilterMethod.HasValue ? columnInfo.FilterMethod.Value : DataGridColumnFilterMethod.Contains)} {columnInfo.SearchValue}");
                    break;
                case DataGridColumnType.Date:
                    var values = columnInfo.SearchValue.ToString().Split("_");
                    if (!string.IsNullOrEmpty(values[0]))
                        filters.Add($"{columnInfo.Field} {Op.GreaterThanOrEqual} {values[0]}");
                    if (!string.IsNullOrEmpty(values[1]))
                        filters.Add($"{columnInfo.Field} {Op.LessThanOrEqual} {values[1]}");
                    break;
                case DataGridColumnType.MultiSelect:
                case DataGridColumnType.Select:
                    // TODO: implementar AnyContains e MultiSelect
                    filters.Add($"{columnInfo.Field}.Id {Op.AnyEquals} {columnInfo.SearchValue}");
                    break;
                case DataGridColumnType.Command:
                    break;
            }
        }
        return string.Join(",", filters);
    }

    protected Task GoBack() => PageHistorysState.GoBack();

    public async Task SingleClicked(DataGridRowMouseEventArgs<TDto> e)
    {
        if (PermissionService.CanWrite)
            ViewEditDeleteButtons.ToEdit();
        else 
            ViewEditDeleteButtons.ToView();
    }

}