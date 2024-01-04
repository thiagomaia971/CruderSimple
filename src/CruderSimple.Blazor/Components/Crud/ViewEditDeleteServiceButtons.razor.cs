using Blazorise;
using Blazorise.DataGrid;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Blazor.Services;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Services;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CruderSimple.Blazor.Components.Crud
{
    public partial class ViewEditDeleteServiceButtons<TEntity, TDto> : ComponentBase 
        where TEntity : IEntity
        where TDto : BaseDto
    {
        [CascadingParameter] public DataGrid<TDto> DataGrid { get; set; }
        [Inject] public PermissionService PermissionService { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] public Blazorise.IMessageService UiMessageService { get; set; }

        [Parameter] public TDto Item { get; set; }

        [Inject] public ICrudService<TEntity, TDto> Service { get; set; }

        [Inject] public INotificationService NotificationService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }

        [Parameter] public bool View { get; set; } = true;

        [Parameter] public bool Edit { get; set; } = true;

        [Parameter] public bool Delete { get; set; } = true;

        private string UrlForView(string id)
            => $"{NavigationManager.Uri}/{id}";

        private string UrlForEdit(string id)
            => $"{NavigationManager.Uri}/{id}/edit";

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        protected override Task OnParametersSetAsync()
        {
            if ( Item is null || Service is null )
                throw new Exception( "Please provide the Item and Service parameters" );
            return base.OnParametersSetAsync();
        }

        public async Task DeleteAsync()
        {
            if (await UiMessageService.Confirm("Deletar esse item?", "Deletar"))
            {
                await Service.Delete(Item.Id);
                await DataGrid.Reload();
                await NotificationService.Success("Deletado com sucesso!");
            }
        }

        public async Task ToView(bool blank = false)
        {
            if (blank)
                await JSRuntime.InvokeAsync<object>("open", UrlForView(Item.Id), "_blank");
            else
                NavigationManager.NavigateTo(UrlForView(Item.Id));
        }

        public async Task ToEdit(bool blank = false)
        {
            if (blank)
                await JSRuntime.InvokeAsync<object>("open", UrlForEdit(Item.Id), "_blank");
            else
                NavigationManager.NavigateTo(UrlForEdit(Item.Id));
        }
    }
}