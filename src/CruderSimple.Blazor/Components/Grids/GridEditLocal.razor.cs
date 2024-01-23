using System;
using System.Reflection.Metadata;
using System.Transactions;
using Blazorise;
using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter( nameof( TEntity ) )]
[CascadingTypeParameter( nameof( TDto ) )]
public partial class GridEditLocal<TEntity, TDto> : CruderGridBase<TEntity, TDto>
    where TEntity : IEntity
    where TDto : BaseDto
{
    public override IList<TDto> AllData 
        => SearchedData
            .Concat(Data)
            .DistinctBy(x => x.Id)
            .Where(x => !x.DeletedAt.HasValue)
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
    public override string StorageKey => $"{base.StorageKey}:{FilterValue}";
    public Modal ModalRef { get; set; }
    public bool IsLoading { get; set; }
    protected Validations ValidationsRef { get; set; }
    public string Errors { get; set; }
    public TDto CurrentSelected { get; set; }
    public bool IsNewModal { get; set; }

    protected override string GetQueryFilter(IEnumerable<DataGridColumnInfo> dataGridColumnInfos, List<string> filters = null) 
        => base.GetQueryFilter(dataGridColumnInfos, [$"{FilterKey} {Op.Equals} {FilterValue}"]);

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
        await DataGridRef.Refresh();
        
        CurrentSelected = null;
    }

    public async Task UpdatingAsync(CancellableRowChange<TDto> context)
    {
        if (await UiMessageService.Confirm("Editar esse item?", "Editar"))
        {
            await Loading.Show();
            await UpdateData(context.NewItem);
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
            await UpdateData(context.NewItem);
            TotalData--;
            await NotificationService.Success("Removido com sucesso!");
            await DataGridRef.Refresh();
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
                    await UpdateData(CurrentSelected);

                await NotificationService.Success($"{(IsNewModal ? "Adicionado" : "Editado")} com sucesso!");
                await ModalRef.Close(CloseReason.None);
            }
            catch (Exception ex)
            {
                Errors = ex.Message;
                await NotificationService.Error(Errors);
            }
            finally
            {
                await DataGridRef.Refresh();
            }
        }
        CurrentSelected = null;
    }

    private async Task AddData(TDto item)
    {
        var dataList = Data.ToList();
        dataList.Add(item);
        Data = dataList;
        TotalData++;
        await DataChanged.InvokeAsync(Data);
    }

    private async Task UpdateData(TDto item)
    {
        var localDataList = Data.ToList();

        localDataList.Remove(localDataList.FirstOrDefault(x => x.GetKey.Equals(item.GetKey)));
        SearchedData.Remove(SearchedData.FirstOrDefault(x => x.GetKey.Equals(item.GetKey)));
        localDataList.Add(item);

        Data = localDataList;
        await DataChanged.InvokeAsync(Data);
        Console.WriteLine(AllData.ToJson());
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

}