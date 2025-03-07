using Blazorise;
using Blazorise.DataGrid;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter(nameof(TGridEntity))]
[CascadingTypeParameter(nameof(TGridDto))]
public partial class CruderGrid<TGridEntity, TGridDto> : CruderGridBase<TGridEntity, TGridDto>
    where TGridEntity : IEntity
    where TGridDto : BaseDto
{
    #region Parameters
    /// <summary>
    /// Data modified will reflected in this property
    /// </summary>
    [Parameter] public IEnumerable<TGridDto> Data { get; set; } = Enumerable.Empty<TGridDto>();

    /// <summary>
    /// two-way-data-bind of Data
    /// </summary>
    [Parameter] public EventCallback<IEnumerable<TGridDto>> DataChanged { get; set; }


    [Parameter] public TGridDto CurrentSelected { get; set; }
    [Parameter] public EventCallback<TGridDto> CurrentSelectedChanged { get; set; }

    /// <summary>
    /// Filter Request API
    /// </summary>
    [Parameter] public string FilterBy { get; set; }

    /// <summary>
    /// Tooltip value for Undo button
    /// </summary>
    [Parameter] public string TooltipUndo { get; set; } = "Restaurar";

    /// <summary>
    /// Tooltip value for Refresh button
    /// </summary>
    [Parameter] public string TooltipRefresh { get; set; } = "Atualizar";

    /// <summary>
    /// Tooltip value for View button
    /// </summary>
    [Parameter] public string TooltipView { get; set; } = "Visualizar";

    /// <summary>
    /// Tooltip value for Edit button
    /// </summary>
    [Parameter] public string TooltipEdit { get; set; } = "Editar";

    /// <summary>
    /// Tooltip value for Delete button
    /// </summary>
    [Parameter] public string TooltipDelete { get; set; } = "Deletar";

    /// <summary>
    /// Tooltip value for New button
    /// </summary>
    [Parameter] public string TooltipNew { get; set; } = "Novo";

    /// <summary>
    /// Modal to create or edit an item. If is null, Add and Edit will be inline.
    /// </summary>
    [Parameter] public RenderFragment<TGridDto> ModalFormTemplate { get; set; }
    [Parameter] public string ModalFormTitle { get; set; }

    /// <summary>
    /// Start new command template
    /// </summary>
    [Parameter] public RenderFragment StartNewCommandTemplate { get; set; }

    /// <summary>
    /// End new command template
    /// </summary>
    [Parameter] public RenderFragment EndNewCommandTemplate { get; set; }

    /// <summary>
    /// Start command template
    /// </summary>
    [Parameter] public RenderFragment<TGridDto> StartCommandTemplate { get; set; }

    /// <summary>
    /// Middle command template
    /// </summary>
    [Parameter] public RenderFragment<TGridDto> MiddleCommandTemplate { get; set; }

    /// <summary>
    /// End command template
    /// </summary>
    [Parameter] public RenderFragment<TGridDto> EndCommandTemplate { get; set; }

    /// <summary>
    /// Event called before the item is created.
    /// </summary>
    [Parameter] public EventCallback<TGridDto> ItemCreating { get; set; }

    /// <summary>
    /// Event called after the item is created.
    /// </summary>
    [Parameter] public EventCallback<TGridDto> ItemCreated { get; set; }

    /// <summary>
    /// Event called before the item is updated.
    /// </summary>
    [Parameter] public EventCallback<(TGridDto OldItem, TGridDto NewItem)> ItemUpdating { get; set; }

    /// <summary>
    /// Event called after the item is updated.
    /// </summary>
    [Parameter] public EventCallback<(TGridDto OldItem, TGridDto NewItem)> ItemUpdated { get; set; }

    /// <summary>
    /// Event called before the item is deleted.
    /// </summary>
    [Parameter] public EventCallback<TGridDto> ItemDeleting { get; set; }

    /// <summary>
    /// Event called after the item is deleted.
    /// </summary>
    [Parameter] public EventCallback<TGridDto> ItemDeleted { get; set; }


    [Parameter] public bool IsDebug { get; set; } = false;


    #endregion Parameters

    #region Properties
    private CruderGridModal<TGridEntity, TGridDto> CruderGridModal { get; set; }
    private bool HasInitialData { get; set; }
    private bool IsInitialDataLoaded { get; set; }
    public bool IsNewItem { get; set; }
    public bool IsEditItem => !IsNewItem;

    private Dictionary<string, (TGridDto Item, bool StyleGrid)> Inserted { get; set; } = new Dictionary<string, (TGridDto, bool StyleGrid)>();
    private Dictionary<string, (TGridDto OldItem, TGridDto NewItem, bool StyleGrid)> BackupModified { get; set; } = new Dictionary<string, (TGridDto, TGridDto, bool StyleGrid)>();
    private Dictionary<string, (TGridDto OldItem, TGridDto NewItem, bool StyleGrid)> BackupDeleted { get; set; } = new Dictionary<string, (TGridDto, TGridDto, bool StyleGrid)>();
    #endregion

    #region Overrides

    protected override Task OnInitializedAsync()
    {
        CruderGridEvents.OnColumnSelected += async (item) =>
        {
            await EditItem(item);
        };

        CruderGridEvents.OnColumnValueChanged += async (oldItem, newItem) =>
        {
            await UpdateItem(oldItem, newItem);
        };

        return base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if ((Data?.Any() ?? false) && !IsInitialDataLoaded)
            HasInitialData = true;

        await base.OnParametersSetAsync();
    }

    protected override string GetQueryFilter(IEnumerable<DataGridColumnInfo> dataGridColumnInfos, List<string> filters = null)
        => base.GetQueryFilter(dataGridColumnInfos, [FilterBy]);

    protected override async Task<List<TGridDto>> FilterData(List<TGridDto> data, GetAllEndpointQuery query = null)
    {
        if (!IsInitialDataLoaded && HasInitialData)
            data = await LoadInitialData(data);
        data = LoadModifiedData(data);
        return data.ToList();
    }

    #endregion

    private async Task<List<TGridDto>> LoadInitialData(List<TGridDto> data)
    {
        IsInitialDataLoaded = true;
        foreach (var itemLoaded in Data)
        {
            var itemSearched = data.FirstOrDefaultByKey(itemLoaded);
            if (itemSearched == null)
                data.Add(itemLoaded);
                //await AddItem(itemLoaded, false);
            else
                data = data.ReplaceItem(itemSearched, itemLoaded).ToList();
                //await UpdateItem(itemSearched, itemLoaded, false);
        }
        return data;
    }

    private List<TGridDto> LoadModifiedData(List<TGridDto> data)
    {
        var insertedOrdered = Inserted.Select(x => x.Value).OrderDescending().Select(x => x.Item);
        data.InsertRange(0, insertedOrdered);
        foreach (var (oldItem, newItem, styleGrid) in BackupModified.Select(x => x.Value))
            data = data.ReplaceItem(oldItem, newItem, styleGrid).ToList();
        foreach (var (oldItem, newItem, styleGrid) in BackupDeleted.Select(x => x.Value))
            data = data.ReplaceItem(oldItem, newItem, styleGrid).ToList();
        return data;
    }

    public async Task CreateItem()
    {
        IsNewItem = true;
        await Select(Activator.CreateInstance<TGridDto>());
        await ItemCreating.InvokeAsync(CurrentSelected);

        if (ModalFormTemplate == null)
            await AddItem(CurrentSelected);
        else
            await CruderGridModal.OpenCreate(CurrentSelected);
    }

    public async Task EditItem(TGridDto item)
    {
        IsNewItem = false;
        var oldEntity = SearchedData.FirstOrDefaultByKey(CurrentSelected);
        await Select(item.Adapt<TGridDto>());
        await ItemUpdating.InvokeAsync((oldEntity, CurrentSelected));

        if (ModalFormTemplate == null)
        {
            UpdateItem(oldEntity, CurrentSelected);

            //if (OnAfterAdd != null)
            //    await OnAfterAdd(CurrentSelected);
        }
        else
        {
            CruderGridModal.OpenEdit(CurrentSelected);
            //if (ModalFormOpened != null)
            //    await ModalFormOpened(CurrentSelected);
        }
    }

    public async Task Select(TGridDto item = null)
    {
        CurrentSelected = item;
        await CurrentSelectedChanged.InvokeAsync(CurrentSelected);
        await DataGridRef.Select(item);
        //await DataGridRef.Refresh();
    }

    public async Task AddItem(TGridDto item, bool style = true)
    {
        if (!Data.Any(x => x.GetKey == item.GetKey))
            await RaiseDataChanged(Data.AddItem(item));

        if (!Inserted.ContainsKey(item.GetKey))
            Inserted.Add(item.GetKey, (item, style));

        SearchedData = await FilterData(SearchedData);
        await ItemCreated.InvokeAsync(item);
    }

    public async Task UpdateItem(TGridDto oldItem, TGridDto newItem, bool style = true)
    {
        //if (OnBeforeUpdate != null)
        //    await OnBeforeUpdate(oldItem, newItem);

        if (Inserted.ContainsKey(oldItem.GetKey))
            Inserted.ReplaceItem(newItem, newItem);
        else if (!BackupModified.TryGetValue(oldItem.GetKey, out var modified))
            BackupModified.Add(oldItem.GetKey, (oldItem, newItem, style));
        else
        {
            var backup = modified;
            BackupModified.Remove(oldItem.GetKey);
            BackupModified.Add(oldItem.GetKey, (backup.OldItem, newItem, true));
        }

        await RaiseDataChanged(Data.ReplaceItem(oldItem, newItem));
        SearchedData = SearchedData.ReplaceItem(oldItem, newItem).ToList();

        await ItemUpdated.InvokeAsync((oldItem, newItem));
        //await DataGridRef.Refresh();

        //if (OnAfterUpdate != null)
        //    await OnAfterUpdate(oldItem, newItem);
    }

    public async Task DeleteItem(TGridDto item)
    {
        var itemToSave = item.Adapt<TGridDto>();
        await ItemUpdating.InvokeAsync((itemToSave, item));
        await ItemDeleting.InvokeAsync(item);

        if (Inserted.ContainsKey(item.GetKey))
        {
            Inserted.Remove(item.GetKey);
            SearchedData.RemoveAll(x => x.GetKey == item.GetKey);
            await RaiseDataChanged(Data.RemoveItem(item));
            return;
        }
        if (BackupModified.ContainsKey(item.GetKey))
        {
            itemToSave = BackupModified[item.GetKey].OldItem;
            BackupModified.Remove(item.GetKey);
            Data = Data.RemoveItem(item);
        }

        item.DeletedAt = DateTime.UtcNow;

        if (!BackupDeleted.ContainsKey(item.GetKey))
            BackupDeleted.Add(item.GetKey, (itemToSave, item, true));

        await RaiseDataChanged(Data.ReplaceItem(item, item));
        await ItemUpdated.InvokeAsync((itemToSave, item));
        await ItemDeleted.InvokeAsync(item);
    }

    protected async Task UndoEdit(EditCommandContext<TGridDto> context)
    {
        if (await UiMessageService.Confirm("Restaurar esse item?", "Restaurar"))
        {
            var itemBackup = BackupModified[context.Item.GetKey];
            BackupModified.Remove(context.Item.GetKey);
            SearchedData = SearchedData.ReplaceItem(context.Item, itemBackup.OldItem).ToList();
            await RaiseDataChanged(Data.RemoveItem(itemBackup.OldItem));
            await DataGridRef.Refresh();
        }
    }

    protected async Task UndoDelete(DeleteCommandContext<TGridDto> context)
    {
        if (await UiMessageService.Confirm("Restaurar esse item?", "Restaurar"))
        {
            context.Item.DeletedAt = null;
            var itemBackup = BackupDeleted[context.Item.GetKey];
            BackupDeleted.Remove(context.Item.GetKey);
            await RaiseDataChanged(Data.RemoveItem(context.Item));
            SearchedData = SearchedData.ReplaceItem(context.Item, itemBackup.OldItem).ToList();
            await DataGridRef.Refresh();
        }
    }

    protected void RowModifiedStyle(TGridDto item, DataGridRowStyling style)
    {
        if (Inserted.TryGetValue(item.GetKey, out var inserted) && inserted.StyleGrid)
            style.Color = Color.Success;
        else if (BackupDeleted.TryGetValue(item.GetKey, out var deleted) && deleted.StyleGrid)
            style.Color = Color.Danger;
        else if (BackupModified.TryGetValue(item.GetKey, out var modified) && modified.StyleGrid)
            style.Color = Color.Warning;
    }

    protected string CalculateWidthCommandColumn()
    {
        var widthBaseSimple = 32;

        var widthFinal = widthBaseSimple;
        if (StartNewCommandTemplate != null)
            widthFinal += widthBaseSimple;
        if (EndNewCommandTemplate != null)
            widthFinal += widthBaseSimple;

        widthFinal += widthBaseSimple;

        return widthFinal.ToString();
    }

    private async Task RaiseDataChanged(IEnumerable<TGridDto> data)
    {
        Data = data;
        await DataChanged.InvokeAsync(Data);
    }
    
}
