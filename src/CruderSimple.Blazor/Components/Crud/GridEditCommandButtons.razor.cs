using Blazorise;
using Blazorise.DataGrid;
using Blazorise.LoadingIndicator;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Services;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace CruderSimple.Blazor.Components.Crud
{
    public partial class GridEditCommandButtons<TEntity, TDto> : ComponentBase
        where TEntity : IEntity
        where TDto : BaseDto
    {

        [CascadingParameter] public DataGrid<TDto> DataGrid { get; set; }

        [CascadingParameter] public LoadingIndicator Loading { get; set; }
        [Parameter] public TDto Item { get; set; }
        [Parameter] public bool View { get; set; } = true;

        [Parameter] public bool Edit { get; set; } = true;

        [Parameter] public bool Delete { get; set; } = true;
        public TDto SelectedItem => DataGrid.SelectedRow;

        [Inject] public PermissionService PermissionService { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] public Blazorise.IMessageService UiMessageService { get; set; }

        [Inject] public ICrudService<TEntity, TDto> Service { get; set; }

        [Inject] public INotificationService NotificationService { get; set; }

        public bool Editing => DataGrid.EditState == DataGridEditState.Edit && SelectedItem?.Id == Item.Id;

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
            if (Item is null || Service is null)
                throw new Exception("Please provide the Item and Service parameters");
            return base.OnParametersSetAsync();
        }

        public async Task CancelAsync()
        {
            await DataGrid.Cancel();
        }

        public async Task DeleteAsync()
        {
            if (await UiMessageService.Confirm("Deletar esse item?", "Deletar"))
            {
                await Loading.Show();
                await Service.Delete(Item.Id);
                await DataGrid.Reload();
                await NotificationService.Success("Deletado com sucesso!");
                await Loading.Hide();
            }
        }


        public async Task ToEdit(MouseEventArgs e)
        {
            await DataGrid.Edit(Item);
        }

        public async Task Save()
        {
            if (await UiMessageService.Confirm("Salvar esse item?", "Salvar"))
            {
                //Console.WriteLine(JsonConvert.SerializeObject(DataGrid.SelectedRow));
                //await Loading.Show();
                //await DataGrid.Save();
                //var result = await Service.Update(Item.Id, SelectedItem);
                //if (result.Success)
                //{
                //    await DataGrid.Save();
                //    await CancelAsync();
                //    await NotificationService.Success("Editado com sucesso!");
                //}
                //await Loading.Hide();
            }
        }
    }
}