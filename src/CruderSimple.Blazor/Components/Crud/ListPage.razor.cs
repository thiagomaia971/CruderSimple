using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Services;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CruderSimple.Blazor.Components.Crud;

[CascadingTypeParameter( nameof( TEntity ) )]
[CascadingTypeParameter( nameof( TDto ) )]
public partial class ListPage<TEntity, TDto> : CruderGridBase<TEntity, TDto>
    where TEntity : IEntity
    where TDto : BaseDto
{
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; }

    public string NewPage => $"{typeof(TEntity).Name}/";

    protected Task GoBack() => PageHistorysState.GoBack();

    private string UrlForView(string id)
        => $"{NavigationManager.Uri}/{id}";

    private string UrlForNew()
        => $"{NavigationManager.Uri}/new";

    private string UrlForEdit(string id)
        => $"{NavigationManager.Uri}/{id}/edit";

    public async Task SingleClicked(DataGridRowMouseEventArgs<TDto> e)
    {
        await ToEdit(e.Item, e.MouseEventArgs.CtrlKey);
    }

    private async Task ToNew(bool blank = false)
    {
        if (!PermissionService.CanWrite)
            await NotificationService.Warning("Não tem permissão de criação/edição!");

        if (blank)
            await JSRuntime.InvokeAsync<object>("open", UrlForNew(), "_blank");
        else
            NavigationManager.NavigateTo(UrlForNew());
    }

    private async Task ToEdit(TDto item, bool blank = false)
    {
        if (PermissionService.CanWrite)
        {
            if (blank)
                await JSRuntime.InvokeAsync<object>("open", UrlForEdit(item.Id), "_blank");
            else
                NavigationManager.NavigateTo(UrlForEdit(item.Id));
        }
        else
            await ToView(item, blank);
    }

    private async Task ToView(TDto item, bool blank = false)
    {
        if (blank)
            await JSRuntime.InvokeAsync<object>("open", UrlForView(item.Id), "_blank");
        else
            NavigationManager.NavigateTo(UrlForView(item.Id));
    }
}