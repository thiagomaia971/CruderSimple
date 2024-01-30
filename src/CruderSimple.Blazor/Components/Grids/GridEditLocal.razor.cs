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

[CascadingTypeParameter( nameof( TEntity ) )]
[CascadingTypeParameter( nameof( TDto ) )]
public partial class GridEditLocal<TEntity, TDto> : CruderGridBase<TEntity, TDto>
    where TEntity : IEntity
    where TDto : BaseDto
{
    public override IList<TDto> AllData 
        => SearchedData
            .DistinctBy(x => x.Id)
            .ToList();

    [Parameter] public IEnumerable<TDto> Data { get; set; } = Enumerable.Empty<TDto>();
    [Parameter] public EventCallback<IEnumerable<TDto>> DataChanged { get; set; }
    [Parameter] public string FilterKey { get; set; }
    [Parameter] public string FilterValue { get; set; }
    [Parameter] public Action<TDto> DefaultNewInstance { get;set; }
    [Parameter] public bool SimpleNewCommand { get; set; }
    [Parameter] public bool EditCommandAllowed { get; set; }
    [Parameter] public string ModalFormTitle { get; set; }
    [Parameter] public string UndoTooltip { get; set; } = "Restaurar";
    [Parameter] public string RefreshTooltip { get; set; } = "Atualizar";
    [Parameter] public string ViewTooltip { get; set; } = "Visualizar";
    [Parameter] public string EditTooltip { get; set; } = "Editar";
    [Parameter] public string DeleteTooltip { get; set; } = "Deletar";
    [Parameter] public string NewTooltip { get; set; } = "Novo";
    [Parameter] public Func<TDto, Task> ItemAdded { get;set; }
    [Parameter] public Func<TDto, Task> ItemUpdated { get;set; }
    [Parameter] public Func<TDto, Task> ModalOpened { get;set; }
    [Parameter] public Func<TDto, Task> OnSaveItem { get;set; }
    [Parameter] public RenderFragment StartNewCommandTemplate { get; set; }
    [Parameter] public RenderFragment EndNewCommandTemplate { get; set; }
    [Parameter] public RenderFragment<TDto> StartCommandTemplate { get; set; }
    [Parameter] public RenderFragment<TDto> MiddleCommandTemplate { get; set; }
    [Parameter] public RenderFragment<TDto> EndCommandTemplate { get; set; }
    [Parameter] public RenderFragment<TDto> ModalForm { get; set; }
    public Modal ModalRef { get; set; }
    public bool IsLoading { get; set; }
    protected Validations ValidationsRef { get; set; }
    public string Errors { get; set; }
    public TDto CurrentSelected { get; set; }
    public bool IsNewModal { get; set; }
    public IList<TDto> Inserted { get; set; } = new List<TDto>();
    private bool HasInitialData { get; set; }
    private bool HasInitialDataLoaded { get;set;}

    public Dictionary<string, (TDto OldItem, TDto NewItem)> BackupModified { get; set;} = new Dictionary<string, (TDto, TDto)>();
    public Dictionary<string, (TDto OldItem, TDto NewItem)> BackupDeleted { get; set; } = new Dictionary<string, (TDto, TDto)>();

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
        if (Data.Any() && !HasInitialDataLoaded)
        {
            HasInitialData = true;
            HasInitialDataLoaded = true;
        }
        await base.OnParametersSetAsync();
    }

    protected override string GetQueryFilter(IEnumerable<DataGridColumnInfo> dataGridColumnInfos, List<string> filters = null) 
        => base.GetQueryFilter(dataGridColumnInfos, [$"{FilterKey} {Op.Equals} {FilterValue}"]);

    protected override async Task<List<TDto>> FilterData(List<TDto> data, GetAllEndpointQuery query = null)
    {
        if (HasInitialData)
        {
            HasInitialData = false;
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
        
        var insertedOrdered = OrderByLocal(Inserted.ToList(), query);
        data.InsertRange(0, insertedOrdered/*.Concat(modifiedOrdered)*//*.Concat(deletedOrdered)*/);
        foreach (var modified in BackupModified)
            data = data.ReplaceItem(modified.Value.NewItem, x => x.GetKey == modified.Value.OldItem.GetKey, false).ToList();
        foreach (var deleted in BackupDeleted)
            data = data.ReplaceItem(deleted.Value.NewItem, x => x.GetKey == deleted.Value.OldItem.GetKey, false).ToList();
        return data.ToList();
    }

    private List<TDto> OrderByLocal(List<TDto> data, GetAllEndpointQuery query) 
        => data.AsQueryable().ApplyOrderBy(query).ToList();

    private async Task OpenCreateMode(NewCommandContext<TDto> command)
    {
        if (ModalForm == null)
        {
            CurrentSelected = Activator.CreateInstance<TDto>();
            await AddData(CurrentSelected);
            //await command.Clicked.InvokeAsync(this);
        }
        else
        {
            IsNewModal = true;
            CurrentSelected = Activator.CreateInstance<TDto>();
            await ModalRef.Show();
            if (ModalOpened != null)
                await ModalOpened(CurrentSelected);
        }

        if (ItemAdded != null)
            await ItemAdded(CurrentSelected);
    }

    public async Task OpenEditMode(TDto e, EditCommandContext<TDto> editContext = null)
    {
        await DataGridRef.Select(e);
        CurrentSelected = e.Adapt<TDto>();
        StateHasChanged();
        if (ModalForm == null)
        {
            await DataGridRef.Edit(e);
            if (editContext?.Clicked != null)
                await editContext?.Clicked.InvokeAsync();
        }
        else
        {
            IsNewModal = false;
            await ModalRef.Show();
            if (ModalOpened != null)
                await ModalOpened(CurrentSelected);
        }
        CruderGridEvents.RaiseOnEditMode();
    }

    public async Task InsertingAsync(CancellableRowChange<TDto> context)
    {
        await Loading.Show();
        await AddData(context.NewItem);
        await NotificationService.Success("Adicionado com sucesso!");
        await Loading.Hide();
        
        CurrentSelected = null;
    }

    public async Task UpdatingAsync(CancellableRowChange<TDto> context)
    {
        await Loading.Show();
        await UpdateData(context.OldItem, context.NewItem);
        if (OnSaveItem != null)
            await OnSaveItem(CurrentSelected);
        await Loading.Hide();

        //await DataGridRef.Refresh();
        CurrentSelected = null;
        await DataGridRef.Select(null);
    }

    protected override async Task RemovingAsync(CancellableRowChange<TDto> context)
    {
        await DeleteData(context.NewItem);
    }

    protected async Task UndoEdit(EditCommandContext<TDto> context)
    {
        if (await UiMessageService.Confirm("Restaurar esse item?", "Restaurar"))
        {
            var itemBackup = BackupModified[context.Item.GetKey];
            BackupModified.Remove(context.Item.GetKey);
            Data = Data.RemoveItem(x => x.GetKey == context.Item.GetKey);
            await DataChanged.InvokeAsync();

            SearchedData = SearchedData.ReplaceItem(itemBackup.OldItem, x => x.GetKey == context.Item.GetKey).ToList();
            await DataGridRef.Refresh();
        }
    }

    protected async Task UndoDelete(DeleteCommandContext<TDto> context)
    {
        if (await UiMessageService.Confirm("Restaurar esse item?", "Restaurar"))
        {
            context.Item.DeletedAt = null;
            var itemBackup = BackupDeleted[context.Item.GetKey];
            BackupDeleted.Remove(context.Item.GetKey);
            Data = Data.RemoveItem(x => x.GetKey ==  context.Item.GetKey);
            await DataChanged.InvokeAsync();

            SearchedData = SearchedData.ReplaceItem(itemBackup.OldItem, x => x.GetKey == context.Item.GetKey).ToList();
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
                if (DefaultNewInstance is not null)
                    DefaultNewInstance(CurrentSelected);

                if (IsNewModal)
                    await AddData(CurrentSelected);
                else
                {
                    if (OnSaveItem != null)
                        await OnSaveItem(CurrentSelected);
                    await UpdateData(SearchedData.FirstOrDefault(x => x.GetKey == CurrentSelected.GetKey), CurrentSelected);
                }

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

    public async Task AddData(TDto item)
    {
        if (OnSaveItem != null)
            await OnSaveItem(item);

        var dataList = Data.ToList();
        dataList.Add(item);
        Data = dataList;
        await DataChanged.InvokeAsync(Data);

        if (!Inserted.Any(x => x.Id == item.Id))
            Inserted.Add(item);

        SearchedData = await FilterData(SearchedData);
        //await DataGridRef.Refresh();
    }

    private async Task UpdateData(TDto oldItem, TDto newItem)
    {
        if (OnSaveItem != null)
            await OnSaveItem(newItem);

        if (Inserted.Any(x => x.GetKey == oldItem.GetKey))
            return;
        if (!BackupModified.ContainsKey(oldItem.GetKey))
            BackupModified.Add(oldItem.GetKey, (oldItem, newItem));
        Data = Data.ReplaceItem(newItem, x => x.GetKey.Equals(oldItem.GetKey));
        SearchedData = SearchedData.ReplaceItem(newItem, x => x.GetKey.Equals(oldItem.GetKey)).ToList();
        await DataChanged.InvokeAsync(Data);
    }

    private async Task DeleteData(TDto item)
    {
        var itemToSave = item.Adapt<TDto>();
        if (BackupModified.ContainsKey(item.GetKey))
        {
            itemToSave = BackupModified[item.GetKey].OldItem;
            BackupModified.Remove(item.GetKey);
            Data = Data.RemoveItem(x => x.GetKey == item.GetKey);
        }
        if (!BackupDeleted.ContainsKey(item.GetKey))
            BackupDeleted.Add(item.GetKey, (itemToSave, item));

        
        item.DeletedAt = DateTime.UtcNow;
        Data = Data.ReplaceItem(item, x => x.GetKey.Equals(item.GetKey));
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
        var widthBaseLarge = 67;

        var widthFinal = 0;
        if (SimpleNewCommand)
            widthFinal += widthBaseSimple;
        else
            widthFinal += widthBaseLarge;
        if (StartNewCommandTemplate != null)
            widthFinal += widthBaseSimple;
        if (EndNewCommandTemplate != null)
            widthFinal += widthBaseSimple;

        widthFinal += widthBaseSimple;

        return widthFinal.ToString();
    }

    protected void RowModifiedStyle(TDto item, DataGridRowStyling style)
    {
        if (Inserted.Any(x => x.Id == item.Id))
            style.Color = Color.Success;
        else if (BackupDeleted.ContainsKey(item.GetKey))
            style.Color = Color.Danger;
        else if (BackupModified.ContainsKey(item.GetKey))
            style.Color = Color.Warning;
    }
}