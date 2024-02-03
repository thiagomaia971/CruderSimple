using System;
using System.Reflection.Metadata;
using Blazorise;
using Blazorise.DataGrid;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter( nameof( TGridEntity ) )]
[CascadingTypeParameter( nameof( TGridDto ) )]
public partial class GridEditLocal<TGridEntity, TGridDto> : CruderGridBase<TGridEntity, TGridDto>
    where TGridEntity : IEntity
    where TGridDto : BaseDto
{
    public override IList<TGridDto> AllData 
        => SearchedData
            .DistinctBy(x => x.Id)
            .ToList();

    [Parameter] public IEnumerable<TGridDto> Data { get; set; } = Enumerable.Empty<TGridDto>();
    [Parameter] public EventCallback<IEnumerable<TGridDto>> DataChanged { get; set; }

    /// <summary>
    /// Filter API by Key
    /// </summary>
    [Parameter] public string FilterKey { get; set; }

    /// <summary>
    /// Filter API by Key Value
    /// </summary>
    [Parameter] public string FilterValue { get; set; }
    [Parameter] public bool EditCommandAllowed { get; set; }
    [Parameter] public string ModalFormTitle { get; set; }
    [Parameter] public string UndoTooltip { get; set; } = "Restaurar";
    [Parameter] public string RefreshTooltip { get; set; } = "Atualizar";
    [Parameter] public string ViewTooltip { get; set; } = "Visualizar";
    [Parameter] public string EditTooltip { get; set; } = "Editar";
    [Parameter] public string DeleteTooltip { get; set; } = "Deletar";
    [Parameter] public string NewTooltip { get; set; } = "Novo";
    [Parameter] public Func<TGridDto, Task> OnBeforeAdd { get;set; }
    [Parameter] public Func<TGridDto, Task> OnAfterAdd { get;set; }
    [Parameter] public Func<TGridDto, TGridDto, Task> OnBeforeUpdate { get;set; }
    [Parameter] public Func<TGridDto, TGridDto, Task> OnAfterUpdate { get;set; }
    [Parameter] public Func<TGridDto, Task> ModalFormOpened { get;set; }
    [Parameter] public Func<TGridDto, Task> OnBeforeSaveItem { get;set; }
    [Parameter] public RenderFragment StartNewCommandTemplate { get; set; }
    [Parameter] public RenderFragment EndNewCommandTemplate { get; set; }
    [Parameter] public RenderFragment<TGridDto> StartCommandTemplate { get; set; }
    [Parameter] public RenderFragment<TGridDto> MiddleCommandTemplate { get; set; }
    [Parameter] public RenderFragment<TGridDto> EndCommandTemplate { get; set; }
    [Parameter] public RenderFragment<TGridDto> ModalForm { get; set; }
    public Modal ModalRef { get; set; }
    protected Validations ValidationsRef { get; set; }
    public string Errors { get; set; }
    public TGridDto CurrentSelected { get; set; }
    public bool IsNewModal { get; set; }
    public IList<TGridDto> Inserted { get; set; } = new List<TGridDto>();
    private bool HasInitialData { get; set; }
    private bool HasInitialDataLoaded { get; set; }

    public Dictionary<string, (TGridDto OldItem, TGridDto NewItem)> BackupModified { get; set;} = new Dictionary<string, (TGridDto, TGridDto)>();
    public Dictionary<string, (TGridDto OldItem, TGridDto NewItem)> BackupDeleted { get; set; } = new Dictionary<string, (TGridDto, TGridDto)>();

    protected override Task OnInitializedAsync()
    {
        CruderGridEvents.OnColumnValueChanged += async (oldItem, newItem) =>
        {
            await UpdateData(oldItem, newItem);
        };
        CruderGridEvents.OnColumnSelected += async (item) =>
        {
            await OpenEditMode(item);
        };

        return base.OnInitializedAsync();
    }


    protected override async Task OnParametersSetAsync()
    {
        if ((Data?.Any() ?? false) && !HasInitialDataLoaded)
        {
            HasInitialData = true;
            HasInitialDataLoaded = true;
        }
        await base.OnParametersSetAsync();
    }

    protected override string GetQueryFilter(IEnumerable<DataGridColumnInfo> dataGridColumnInfos, List<string> filters = null) 
        => base.GetQueryFilter(dataGridColumnInfos, [$"{FilterKey} {Op.Equals} {FilterValue}"]);

    protected override async Task<List<TGridDto>> FilterData(List<TGridDto> data, GetAllEndpointQuery query = null)
    {
        if (HasInitialData)
        {
            HasInitialData = false;
            //Console.WriteLine(Data.ToJson());
            foreach (var itemLoaded in Data) 
            {
                var itemSearched = data.FirstOrDefault(x => x.GetKey == itemLoaded.GetKey);
                if (itemSearched == null)
                    await AddData(itemLoaded);
                else
                    await UpdateData(itemSearched, itemLoaded);
            }
        }

        if (query == null)
            query = CreateQuery((await LoadColumnsFromLocalStorage())
                .Select(x => new DataGridColumnInfo(
                    x.Field,
                    x.Filter?.SearchValue,
                    x.CurrentSortDirection,
                    x.CurrentSortDirection != SortDirection.Default ? 0 : -1,
                    DataGridColumnType.Text,
                    x.SortField,
                    x.Filter?.FilterMethod ?? DataGridColumnFilterMethod.Contains)));
        
        var insertedOrdered = Inserted.OrderDescending(); //OrderByLocal(Inserted.ToList(), query);
        data.InsertRange(0, insertedOrdered/*.Concat(modifiedOrdered)*//*.Concat(deletedOrdered)*/);
        foreach (var modified in BackupModified)
            data = data.ReplaceItem(modified.Value.OldItem, modified.Value.NewItem, false).ToList();
        foreach (var deleted in BackupDeleted)
            data = data.ReplaceItem(deleted.Value.OldItem, deleted.Value.NewItem, false).ToList();
        return data.ToList();
    }

    private List<TGridDto> OrderByLocal(List<TGridDto> data, GetAllEndpointQuery query) 
        => data.AsQueryable().ApplyOrderBy(query).ToList();

    private async Task OpenCreateMode(NewCommandContext<TGridDto> command)
    {
        CurrentSelected = Activator.CreateInstance<TGridDto>();

        if (OnBeforeAdd != null)
            await OnBeforeAdd(CurrentSelected);

        if (ModalForm == null)
        {
            await AddData(CurrentSelected);

            if (OnAfterAdd != null)
                await OnAfterAdd(CurrentSelected);
        }
        else
        {
            IsNewModal = true;
            await ModalRef.Show();
            if (ModalFormOpened != null)
                await ModalFormOpened(CurrentSelected);
        }
    }

    public async Task OpenEditMode(TGridDto e, EditCommandContext<TGridDto> editContext = null)
    {
        var entityToEdit = e;

        if (Inserted.Any(x => x.GetKey == e.GetKey))
            entityToEdit = Inserted.FirstOrDefault(x => x.GetKey == e.GetKey);
        else if (BackupModified.ContainsKey(e.GetKey))
            entityToEdit = BackupModified[e.GetKey].NewItem;

        CurrentSelected = entityToEdit.Adapt<TGridDto>();

        await DataGridRef.Select(CurrentSelected);
        StateHasChanged();
        if (ModalForm == null)
        {
            await DataGridRef.Edit(CurrentSelected);
            if (editContext?.Clicked != null)
                await editContext?.Clicked.InvokeAsync();
        }
        else
        {
            IsNewModal = false;
            await ModalRef.Show();
            if (ModalFormOpened != null)
                await ModalFormOpened(CurrentSelected);
        }
        CruderGridEvents.RaiseOnEditMode();
    }

    public async Task InsertingAsync(CancellableRowChange<TGridDto> context)
    {
        await Loading.Show();
        await AddData(context.NewItem);
        await Loading.Hide();
        
        CurrentSelected = null;
        await DataGridRef.Select(null);
    }

    public async Task UpdatingAsync(CancellableRowChange<TGridDto> context)
    {
        await Loading.Show();
        await UpdateData(context.OldItem, context.NewItem);
        await Loading.Hide();

        CurrentSelected = null;
        await DataGridRef.Select(null);
    }

    protected override async Task RemovingAsync(CancellableRowChange<TGridDto> context)
    {
        await DeleteData(context.NewItem);
    }

    protected async Task UndoEdit(EditCommandContext<TGridDto> context)
    {
        if (await UiMessageService.Confirm("Restaurar esse item?", "Restaurar"))
        {
            var itemBackup = BackupModified[context.Item.GetKey];
            BackupModified.Remove(context.Item.GetKey);
            Data = Data.RemoveItem(context.Item);
            await DataChanged.InvokeAsync();

            SearchedData = SearchedData.ReplaceItem(context.Item, itemBackup.OldItem).ToList();
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
            Data = Data.RemoveItem(context.Item);
            await DataChanged.InvokeAsync();

            SearchedData = SearchedData.ReplaceItem(context.Item, itemBackup.OldItem).ToList();
            await DataGridRef.Refresh();
        }
    }

    protected async Task SaveModal()
    {
        if (ValidationsRef is not null && await ValidationsRef.ValidateAll())
        {
            Errors = null;
            try
            {
                if (OnBeforeSaveItem != null)
                    await OnBeforeSaveItem(CurrentSelected);

                if (IsNewModal)
                    await AddData(CurrentSelected);
                else
                    await UpdateData(SearchedData.FirstOrDefault(x => x.GetKey == CurrentSelected.GetKey), CurrentSelected);

                
                await NotificationService.Success($"{(IsNewModal ? "Adicionado" : "Editado")} com sucesso!");
                await ModalRef.Close(CloseReason.None);
                CurrentSelected = null;
                await DataGridRef.Refresh();
                await DataGridRef.Select(null);
            }
            catch (Exception ex)
            {
                Errors = ex.Message;
                await NotificationService.Error(Errors);
            }
        }
    }

    public async Task AddData(TGridDto item)
    {
        if (!Data.Any(x => x.GetKey == item.GetKey))
        {
            Data = Data.AddItem(item);
            await DataChanged.InvokeAsync(Data);
        }

        if (!Inserted.Any(x => x.Id == item.Id))
            Inserted.Insert(0, item);

        SearchedData = await FilterData(SearchedData);
    }

    public async Task UpdateData(TGridDto oldItem, TGridDto newItem)
    {
        if (OnBeforeUpdate != null)
            await OnBeforeUpdate(oldItem, newItem);

        if (Inserted.Any(x => x.GetKey == oldItem.GetKey))
            Inserted = Inserted.ReplaceItem(newItem, newItem).ToList();
        else if (!BackupModified.ContainsKey(oldItem.GetKey))
            BackupModified.Add(oldItem.GetKey, (oldItem, newItem));

        Data = Data.ReplaceItem(oldItem, newItem);
        SearchedData = SearchedData.ReplaceItem(oldItem, newItem).ToList();
        await DataChanged.InvokeAsync(Data);
        await DataGridRef.Refresh();

        if (OnAfterUpdate != null)
            await OnAfterUpdate(oldItem, newItem);
    }

    private async Task DeleteData(TGridDto item)
    {
        var itemToSave = item.Adapt<TGridDto>();
        if (Inserted.Any(x => x.GetKey == item.GetKey))
        {
            Inserted.Remove(Inserted.FirstOrDefault(x => x.GetKey == item.GetKey));
            SearchedData.RemoveAll(x => x.GetKey == item.GetKey);
            Data = Data.RemoveItem(item);
            await DataChanged.InvokeAsync(Data);
            await DataGridRef.Refresh();
            Console.WriteLine(SearchedData.ToJson());
            return;
        }
        if (BackupModified.ContainsKey(item.GetKey))
        {
            itemToSave = BackupModified[item.GetKey].OldItem;
            BackupModified.Remove(item.GetKey);
            Data = Data.RemoveItem(item);
        }
        if (!BackupDeleted.ContainsKey(item.GetKey))
            BackupDeleted.Add(item.GetKey, (itemToSave, item));

        
        item.DeletedAt = DateTime.UtcNow;
        Data = Data.ReplaceItem(item, item);
        await DataChanged.InvokeAsync(Data);
        await DataGridRef.Refresh();
    }

    protected async Task ModalClosed(ModalClosingEventArgs e)
    {
        await DataGridRef.Select(null);
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

    protected void RowModifiedStyle(TGridDto item, DataGridRowStyling style)
    {
        if (Inserted.Any(x => x.Id == item.Id))
            style.Color = Color.Success;
        else if (BackupDeleted.ContainsKey(item.GetKey))
            style.Color = Color.Danger;
        else if (BackupModified.ContainsKey(item.GetKey))
            style.Color = Color.Warning;
    }
}