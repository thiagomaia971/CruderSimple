using Blazorise;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Services;
using CruderSimple.Core.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter(nameof(TGridEntity))]
[CascadingTypeParameter(nameof(TGridDto))]
public partial class CruderGridModal<TGridEntity, TGridDto> : ComponentBase
    where TGridEntity : IEntity
    where TGridDto : BaseDto
{
    [CascadingParameter] public CruderGrid<TGridEntity, TGridDto> CruderGrid { get; set; }

    /// <summary>
    /// Modal to create or edit an item. If is null, Add and Edit will be inline.
    /// </summary>
    [Parameter] public RenderFragment<TGridDto> ModalFormTemplate { get; set; }
    [Parameter] public string ModalFormTitle { get; set; }

    [Inject] public INotificationService NotificationService { get; set; }
    [Inject] public PermissionService PermissionService { get; set; }


    public TGridDto CurrentSelected => CruderGrid?.CurrentSelected;
    public TGridDto CurrentSelectedBackup { get; set; }
    private bool IsNewItem { get; set; }
    protected bool IsLoading { get; set; }

    private Modal ModalRef { get; set; }
    protected Validations ValidationsRef { get; set; }
    private string Errors { get; set; }

    public async Task OpenCreate(TGridDto item)
    {
        IsNewItem = true;
        CurrentSelectedBackup = item.Adapt<TGridDto>();
        await ModalRef.Show();
    }

    public async Task OpenEdit(TGridDto item)
    {
        IsNewItem = false;
        CurrentSelectedBackup = item.Adapt<TGridDto>();
        await ModalRef.Show();
    }

    public async Task SaveModal()
    {
        if (ValidationsRef is not null && await ValidationsRef.ValidateAll())
        {
            Errors = null;
            try
            {
                //if (OnBeforeSaveItem != null)
                //    await OnBeforeSaveItem(CurrentSelected);

                if (IsNewItem)
                    await CruderGrid.AddItem(CurrentSelected, true);
                else
                    await CruderGrid.UpdateItem(CurrentSelectedBackup, CurrentSelected, true);
                await CruderGrid.Select(null);

                //await NotificationService.Success($"{(IsNewModal ? "Adicionado" : "Editado")} com sucesso!");
                await ModalRef.Close(CloseReason.None);
                //CurrentSelected = null;
                //await DataGridRef.Refresh();
                //await DataGridRef.Select(null);
            }
            catch (Exception ex)
            {
                Errors = ex.Message;
                await NotificationService.Error(Errors);
            }
        }
    }

    protected async Task ModalClosed(ModalClosingEventArgs e)
    {
        await CruderGrid.Select(null);
    }
}
