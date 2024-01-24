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
            .ToList();

    [Parameter] public IEnumerable<TDto> Data { get; set; } = Enumerable.Empty<TDto>();
    [Parameter] public EventCallback<IEnumerable<TDto>> DataChanged { get; set; }
    [Parameter] public string FilterKey { get; set; }
    [Parameter] public string FilterValue { get; set; }
    [Parameter] public Action<TDto> DefaultNewInstance { get;set; }
    [Parameter] public bool SimpleNewCommand { get; set; }
    [Parameter] public bool EditCommandAllowed { get; set; }
    [Parameter] public string ModalFormTitle { get; set; }
    [Parameter] public bool IsLocal { get; set; }
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
    public IList<TDto> Modified { get; set; } = new List<TDto>();
    public IList<TDto> Deleted { get; set; } = new List<TDto>();

    protected override string GetQueryFilter(IEnumerable<DataGridColumnInfo> dataGridColumnInfos, List<string> filters = null) 
        => base.GetQueryFilter(dataGridColumnInfos, [$"{FilterKey} {Op.Equals} {FilterValue}"]);

    protected override async Task<List<TDto>> FilterData(List<TDto> data, GetAllEndpointQuery query = null)
    {
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
        var modifiedOrdered = OrderByLocal(Modified.ToList(), query);
        var deletedOrdered = OrderByLocal(Deleted.ToList(), query);
        data.InsertRange(0, insertedOrdered.Concat(modifiedOrdered).Concat(deletedOrdered));
        return data.DistinctBy(x => x.Id).ToList();
    }

    private List<TDto> OrderByLocal(List<TDto> data, GetAllEndpointQuery query) 
        => data.AsQueryable().ApplyOrderBy(query).ToList();

    public async Task SingleClicked(TDto e, EditCommandContext<TDto> editContext = null)
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
        }
        CruderGridEvents.RaiseOnEditMode();
    }

    public async Task NewCommand(NewCommandContext<TDto> command)
    {
        if (ModalForm == null) 
        {
            await command.Clicked.InvokeAsync(this);
        }
        else
        {
            IsNewModal = true;
            CurrentSelected = Activator.CreateInstance<TDto>();
            await ModalRef.Show();
        }
    }

    public async Task InsertingAsync(CancellableRowChange<TDto> context)
    {
        if (await UiMessageService.Confirm("Adicionar esse item?", "Adicionar"))
        {
            await Loading.Show();
            await AddData(context.NewItem);
            await NotificationService.Success("Adicionado com sucesso!");
            await Loading.Hide();
        }
        else
            context.Cancel = true;
        //await DataGridRef.Refresh();
        
        CurrentSelected = null;
    }

    public async Task UpdatingAsync(CancellableRowChange<TDto> context)
    {
        if (await UiMessageService.Confirm("Editar esse item?", "Editar"))
        {
            await Loading.Show();
            await UpdateData(context.NewItem, false);
            await NotificationService.Success("Editado com sucesso!");
            await Loading.Hide();
        }
        else
            context.Cancel = true;
        await DataGridRef.Refresh();
        CurrentSelected = null;
    }

    protected override async Task RemovingAsync(CancellableRowChange<TDto> context)
    {
        if (await UiMessageService.Confirm("Remover esse item?", "Remover"))
        {
            context.NewItem.DeletedAt = DateTime.UtcNow;
            await UpdateData(context.NewItem, true);
            await NotificationService.Success("Removido com sucesso!");
        }
        else
            context.Cancel = true;
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
                    await UpdateData(CurrentSelected, false);

                await NotificationService.Success($"{(IsNewModal ? "Adicionado" : "Editado")} com sucesso!");
                await ModalRef.Close(CloseReason.None);
            }
            catch (Exception ex)
            {
                Errors = ex.Message;
                await NotificationService.Error(Errors);
            }
        }
        CurrentSelected = null;
    }

    private async Task AddData(TDto item)
    {
        var dataList = Data.ToList();
        dataList.Add(item);
        Data = dataList;
        await DataChanged.InvokeAsync(Data);

        if (!Inserted.Any(x => x.Id == item.Id))
            Inserted.Add(item);

        SearchedData = await FilterData(SearchedData);
        //await DataGridRef.Refresh();
    }

    private async Task UpdateData(TDto item, bool isRemove)
    {
        var localDataList = Data.ToList();

        var index = SearchedData.IndexOf(SearchedData.FirstOrDefault(x => x.GetKey.Equals(item.GetKey)));
        localDataList.Remove(localDataList.FirstOrDefault(x => x.GetKey.Equals(item.GetKey)));
        SearchedData.Remove(SearchedData.FirstOrDefault(x => x.GetKey.Equals(item.GetKey)));

        localDataList.Add(item);
        if (!isRemove)
            SearchedData.Insert(index, item);

        Data = localDataList;
        await DataChanged.InvokeAsync(Data);

        if (isRemove && !Deleted.Any(x => x.Id == item.Id))
            Deleted.Add(item);
        if (!isRemove && !Modified.Any(x => x.Id == item.Id))
            Modified.Add(item);

        SearchedData = await FilterData(SearchedData);
        await DataGridRef.Refresh();
    }

    protected async Task ModalClosed(ModalClosingEventArgs e)
    {
        //DataGridRef.SelectedRow = null;
        //Console.WriteLine(e.CloseReason);
        //if (e.CloseReason == CloseReason.None)
        //{
        //    await DataGridRef.Refresh();
        //    await DataGridRef.Reload();
        //}
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
        if (Modified.Any(x => x.Id == item.Id))
            style.Color = Color.Warning;
        if (Deleted.Any(x => x.Id == item.Id))
            style.Color = Color.Danger;
    }

}