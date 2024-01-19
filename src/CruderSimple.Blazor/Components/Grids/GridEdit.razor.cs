using System;
using System.Reflection.Metadata;
using System.Transactions;
using Blazorise;
using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter( nameof( TEntity ) )]
[CascadingTypeParameter( nameof( TDto ) )]
public partial class GridEdit<TEntity, TDto> : CruderGridBase<TEntity, TDto>
    where TEntity : IEntity
    where TDto : BaseDto
{
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
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

    public async Task SingleClicked(TDto e, EditCommandContext<TDto> editContext = null)
    {
        await DataGridRef.Select(e);
        CurrentSelected = e;
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
        if (await UiMessageService.Confirm("Salvar esse item?", "Salvar"))
        {
            await Loading.Show();
            var result = await Service.Create(context.NewItem);
            if (result.Success)
                await NotificationService.Success("Adicionado com sucesso!");
            else
                context.Cancel = true;
            await Loading.Hide();
        }
        else
            context.Cancel = true;
        await DataGridRef.Refresh();
        CurrentSelected = null;
    }

    public async Task UpdatingAsync(CancellableRowChange<TDto> context)
    {
        if (await UiMessageService.Confirm("Salvar esse item?", "Salvar"))
        {
            await Loading.Show();
            var result = await Service.Update(context.NewItem.Id, context.NewItem);

            if (result.Success)
                await NotificationService.Success("Atualizado com sucesso!");
            else
                context.Cancel = true;
            await Loading.Hide();
        }
        else
            context.Cancel = true;
        await DataGridRef.Refresh();
        CurrentSelected = null;
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

    protected async Task SaveModal()
    {
        IsLoading = true;
        if (ValidationsRef is not null && await ValidationsRef.ValidateAll())
        {
            Errors = null;
            try
            {
                Result<TDto> result = null;
                if (DefaultNewInstance is not null)
                    DefaultNewInstance(CurrentSelected);
                if (IsNewModal)
                    result = await Service.Create(CurrentSelected);
                else
                    result = await Service.Update(CurrentSelected.Id, CurrentSelected);

                if (result.Success)
                {
                    await NotificationService.Success($"{(IsNewModal ? "Cadastrado" : "Atualizado")} com sucesso!");
                    await ModalRef.Close(CloseReason.None);
                }

            }
            catch (Exception ex)
            {
                Errors = ex.Message;
                await NotificationService.Error(Errors);
            }
            finally
            {
                IsLoading = false;
            }
        }
        IsLoading = false;
        CurrentSelected = null;
    }

    protected async Task ModalClosed(ModalClosingEventArgs e)
    {
        DataGridRef.SelectedRow = null;
        Console.WriteLine(e.CloseReason);
        if (e.CloseReason == CloseReason.None)
        {
            await DataGridRef.Refresh();
            await DataGridRef.Reload();
        }
    }

}