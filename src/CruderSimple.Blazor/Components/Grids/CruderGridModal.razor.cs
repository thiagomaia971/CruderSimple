using Blazorise;
using CruderSimple.Blazor.Services;
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

    [Parameter] public bool IsModalOpen { get; set; }
    /// <summary>
    /// Modal to create or edit an item. If is null, Add and Edit will be inline.
    /// </summary>
    [Parameter] public RenderFragment<TGridDto> ModalFormTemplate { get; set; }
    [Parameter] public string ModalFormTitle { get; set; }

    [Parameter] public TGridDto CurrentSelected { get; set; }
    [Parameter] public bool IsNewItem { get; set; }

    [Inject] public CruderLogger<CruderGridModal<TGridEntity, TGridDto>> Logger { get; set; }
    [Inject] public INotificationService NotificationService { get; set; }
    [Inject] public PermissionService PermissionService { get; set; }

    [CascadingParameter] public WindowDimension Dimension { get; set; }
    public TGridDto CurrentSelectedBackup { get; set; }
    protected bool IsLoading { get; set; }
    private bool IsOpen { get; set; }
    public Dictionary<string, object> CloseButtonAttributes { get; set; } = new Dictionary<string, object> 
    { 
        { "data-dismiss", "modal" } 
    };

    private Modal ModalRef { get; set; }
    protected Validations ValidationsRef { get; set; }
    private string Errors { get; set; }
    public double ModalPercentage { get; set; } = 0.85;

    public int CalculateBy(int? value)
        => value.HasValue ? (int)(value * ModalPercentage) : 0;
    public int CalculateMarginBy(int? value)
        => value.HasValue ? (int)(value * ((1 - ModalPercentage) / 2)) : 0;

    public int ModalBodyHeight => CalculateBy(Dimension?.Heigth) - (CalculateMarginBy(Dimension?.Heigth) * 2) - 15;

    protected override Task OnParametersSetAsync()
    {
        if (CurrentSelected != null)
            CurrentSelectedBackup = CurrentSelected.Adapt<TGridDto>();
        return base.OnParametersSetAsync();
    }

    public async Task OpenCreate(TGridDto item)
    {
        Logger.LogDebug("OpenCreate");
        IsNewItem = true;
        CurrentSelected = item;
        CurrentSelectedBackup = item.Adapt<TGridDto>();
        IsOpen = true;
        StateHasChanged();
    }

    public async Task OpenEdit(TGridDto item)
    {
        Logger.Watch("OpenEdit", () =>
        {
            IsNewItem = false;
            CurrentSelected = item;
            CurrentSelectedBackup = item.Adapt<TGridDto>();
            IsOpen = true;
            StateHasChanged();
        });
    }

    public async Task SaveModal()
    {
        if (ValidationsRef is not null && await ValidationsRef.ValidateAll())
        {
            Errors = null;
            try
            {
                if (IsNewItem)
                    await CruderGrid.AddItem(CurrentSelectedBackup, true);
                else
                    await CruderGrid.UpdateItem(CurrentSelected, CurrentSelectedBackup, true);

                await CruderGrid.Select(null);
                //await ModalRef.Close(CloseReason.None);
                await CruderGrid.Refresh();
                //IsOpen = false;
                //StateHasChanged();

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
