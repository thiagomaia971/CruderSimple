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
using System.Reflection;

namespace CruderSimple.Blazor.Components.Grids
{
    public class CruderGridBase<TEntity, TDto> : ComponentBase
    where TEntity : IEntity
    where TDto : BaseDto
    {
        [Parameter] public RenderFragment Columns { get; set; }
        [Parameter] public RenderFragment<TDto> DetailRowTemplate { get; set; }
        [Parameter] public bool SelectAll { get; set; }
        [Parameter] public string CustomSelect { get; set; }
        [Parameter] public IFluentSizing Height { get; set; } = Blazorise.Height.Px(430);
        [Parameter] public IFluentSpacing Padding { get; set; }

        [CascadingParameter] public LoadingIndicator Loading { get; set; }

        [Inject] public PermissionService PermissionService { get; set; }
        [Inject] public ICrudService<TEntity, TDto> Service { get; set; }
        [Inject] public PageHistoryState PageHistorysState { get; set; }
        [Inject] protected virtual ILocalStorageService LocalStorage { get; set; }
        [Inject] public INotificationService NotificationService { get; set; }
        [Inject] public IMessageService UiMessageService { get; set; }

        public virtual IList<TDto> AllData => SearchedData?.ToList();
        public List<TDto> SearchedData { get; set; } = new List<TDto>();
        public int TotalData { get; set; }
        private DataGrid<TDto> _dataGridRef { get; set; }
        public DataGrid<TDto> DataGridRef { get => _dataGridRef; set 
            { 
                _dataGridRef = value; 
                if (_dataGridRef is not null)
                    if (_dataGridRef.Attributes is null)
                        _dataGridRef.Attributes = new Dictionary<string, object>
                        {
                            { "Events", CruderGridEvents }
                        };
                    else 
                        _dataGridRef.Attributes.Add("Events", CruderGridEvents);
            } 
        }
        public bool IsFirstRender { get; set; } = true;
        public virtual string StorageKey => $"{GetType().Name}<{typeof(TEntity).Name},{typeof(TDto).Name}>";
        public CruderGridEvents CruderGridEvents { get; set; } = new CruderGridEvents();


        protected virtual async Task GetData(DataGridReadDataEventArgs<TDto> e)
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

        protected virtual async Task Search(DataGridReadDataEventArgs<TDto> e)
        {
            var select = GetQuerySelect(e.Columns);
            var filter = GetQueryFilter(e.Columns);
            var orderByColumn = e.Columns.FirstOrDefault(x => x.SortIndex == 0);
            var orderBy = orderByColumn is null ? null : $"{orderByColumn.SortField} {orderByColumn.SortDirection}";

            var data = await Service.GetAll(new GetAllEndpointQuery(select, filter, orderBy, e.PageSize, e.Page));

            TotalData = data.Size;
            SearchedData = data.Data.ToList();
            await SaveColumns();
        }

        protected virtual string GetQuerySelect(IEnumerable<DataGridColumnInfo> dataGridFields)
        {
            if (SelectAll)
                return "*";

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

        protected virtual string GetQueryFilter(IEnumerable<DataGridColumnInfo> dataGridColumnInfos, List<string> filters = null)
        {
            if (filters is null)
                filters = new List<string>();

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
                        // Field is a IEntity or Enumerable<IEntity>

                        var selectColumn = (DataGridSelectColumn<TDto>)DataGridRef.GetColumns().FirstOrDefault(x => x.Field == columnInfo.Field);
                        var itemType = selectColumn.GetType().GenericTypeArguments[0];
                        var property = itemType.GetProperty(selectColumn.Field);
                        var field = (string)selectColumn.Attributes["Field"];

                        if (property.PropertyType.IsEnumerableType(out _))
                        {
                            if (field != null)
                                filters.Add($"{field} {Op.AnyEquals} {searchValues[0]}");
                            else
                                filters.Add($"{columnInfo.Field}.Id {Op.AnyEquals} {searchValues[0]}");
                        }
                        else
                        {
                            if (field != null)
                                filters.Add($"{field} {Op.Equals} {searchValues[0]}");
                            else
                                filters.Add($"{selectColumn.Field}.Id {Op.Equals} {searchValues[0]}");
                        }
                        break;
                    case DataGridColumnType.Command:
                        break;
                }
            }
            return string.Join(",", filters);
        }

        protected virtual async Task LoadColumns()
        {
            var data = await LocalStorage.GetItemAsync<ListPageLocalStorage<TDto>>(StorageKey);
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
            CruderGridEvents.RaiseOnColumnsLoaded();
        }

        protected virtual async Task SaveColumns()
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
            await LocalStorage.SetItemAsync(StorageKey, data);
        }

        protected virtual async Task RemovingAsync(CancellableRowChange<TDto> context)
        {
            if (await UiMessageService.Confirm("Deletar esse item?", "Deletar"))
            {
                await Loading.Show();
                var result = await Service.Delete(context.NewItem.Id);
                if (result.Success)
                {
                    await NotificationService.Success("Deletado com sucesso!");
                    await DataGridRef.Reload();
                }
                else
                    context.Cancel = true;
                await Loading.Hide();
            }
            else
                context.Cancel = true;
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
        public string SelectItem { get; set; }
        public DataGridColumnFilterMethod FilterMethod { get; set; }
    }

    public delegate void ColumnsLoaded();
    public delegate void EditMode();
    public class CruderGridEvents
    {
        public event ColumnsLoaded OnColumnsLoaded;
        public event EditMode OnEditMode;

        public void RaiseOnColumnsLoaded()
        {
            if (OnColumnsLoaded != null)
                OnColumnsLoaded();
        }

        public void RaiseOnEditMode()
        {
            if (OnEditMode != null)
                OnEditMode();
        }
    }
}
