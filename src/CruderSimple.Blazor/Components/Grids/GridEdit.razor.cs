using System.Reflection.Metadata;
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
    [Parameter] public RenderFragment StartNewCommandTemplate { get; set; }
    [Parameter] public RenderFragment EndNewCommandTemplate { get; set; }
    [Parameter] public RenderFragment<TDto> StartCommandTemplate { get; set; }
    [Parameter] public RenderFragment<TDto> MiddleCommandTemplate { get; set; }
    [Parameter] public RenderFragment<TDto> EndCommandTemplate { get; set; }
    public override string StorageKey => $"{base.StorageKey}:{FilterValue}";

    protected override string GetQueryFilter(IEnumerable<DataGridColumnInfo> dataGridColumnInfos, List<string> filters = null) 
        => base.GetQueryFilter(dataGridColumnInfos, [$"{FilterKey} {Op.Equals} {FilterValue}"]);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    { 
        await base.OnAfterRenderAsync(firstRender);
    }

    public async Task SingleClicked(DataGridRowMouseEventArgs<TDto> e)
    {
        await DataGridRef.Select(e.Item);
        await DataGridRef.Edit(e.Item);
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
    }

}