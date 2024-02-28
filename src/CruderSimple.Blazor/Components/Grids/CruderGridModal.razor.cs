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
    #region Parameters

    /// <summary>
    /// Modal to create or edit an item. If is null, Add and Edit will be inline.
    /// </summary>
    [Parameter] public RenderFragment<TGridDto> ModalFormTemplate { get; set; }

    /// <summary>
    /// Modal title header
    /// </summary>
    [Parameter] public string ModalFormTitle { get; set; }

    [CascadingParameter] public WindowDimension Dimension { get; set; }

    #endregion

    //public TGridDto CurrentSelected { get; set; }

    //[Inject] public CruderLogger<CruderGridModal<TGridEntity, TGridDto>> Logger { get; set; }
    //[Inject] public INotificationService NotificationService { get; set; }
    //[Inject] public PermissionService PermissionService { get; set; }

    //public TGridDto CurrentSelectedBackup { get; set; }
    //protected bool IsLoading { get; set; }
    //public Dictionary<string, object> CloseButtonAttributes { get; set; } = new Dictionary<string, object> 
    //{ 
    //    { "data-dismiss", "modal" } 
    //};

    //protected Validations ValidationsRef { get; set; }
    //private string Errors { get; set; }

    #region Injects
    #endregion

    #region Properties
    public double ModalPercentage { get; set; } = 0.85;

    private int CalculateBy(int? value)
        => value.HasValue ? (int)(value * ModalPercentage) : 0;
    private int CalculateMarginBy(int? value)
        => value.HasValue ? (int)(value * ((1 - ModalPercentage) / 2)) : 0;

    private int ModalBodyHeight => CalculateBy(Dimension?.Heigth) - (CalculateMarginBy(Dimension?.Heigth) * 2) - 15;
    private bool IsOpen { get; set; }
    private bool IsNewItem { get; set; }

    #endregion

    public async Task ShowCreate()
    {
        IsNewItem = true;
        await Show();
    }

    public async Task ShowEdit()
    {
        IsNewItem = false;
        await Show();
    }

    private async Task Show()
    {
        IsOpen = true;
        StateHasChanged();
    }

    //protected override Task OnParametersSetAsync()
    //{
    //    if (CurrentSelected != null)
    //        CurrentSelectedBackup = CurrentSelected.Adapt<TGridDto>();
    //    return base.OnParametersSetAsync();
    //}

    //public async Task OpenCreate(TGridDto item)
    //{
    //    Logger.LogDebug("OpenCreate");
    //    IsNewItem = true;
    //    CurrentSelected = item;
    //    CurrentSelectedBackup = item.Adapt<TGridDto>();
    //    IsOpen = true;
    //    StateHasChanged();
    //}

    //public async Task OpenEdit(TGridDto item)
    //{
    //    Logger.Watch("OpenEdit", () =>
    //    {
    //        IsNewItem = false;
    //        CurrentSelected = item;
    //        CurrentSelectedBackup = item.Adapt<TGridDto>();
    //        IsOpen = true;
    //        //StateHasChanged();
    //    });
    //}

    //public async Task SaveModal()
    //{
    //    if (ValidationsRef is not null && await ValidationsRef.ValidateAll())
    //    {
    //        Errors = null;
    //        try
    //        {
    //            if (IsNewItem)
    //                await CruderGrid.AddItem(CurrentSelectedBackup, true);
    //            else
    //                await CruderGrid.UpdateItem(CurrentSelected, CurrentSelectedBackup, true);

    //            await CruderGrid.Select(null);
    //            //await ModalRef.Close(CloseReason.None);
    //            await CruderGrid.Refresh();
    //            //IsOpen = false;
    //            //StateHasChanged();

    //        }
    //        catch (Exception ex)
    //        {
    //            Errors = ex.Message;
    //            await NotificationService.Error(Errors);
    //        }
    //    }
    //}

    protected async Task ModalClosed(ModalClosingEventArgs e)
    {
        IsOpen = false;
    }
}
