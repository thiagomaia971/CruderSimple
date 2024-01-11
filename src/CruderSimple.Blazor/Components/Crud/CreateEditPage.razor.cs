using Blazorise;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Blazor.Services;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Services;
using CruderSimple.Core.ViewModels;
using Force.DeepCloner;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CruderSimple.Blazor.Components.Crud;

[CascadingTypeParameter( nameof( TEntity ) )]
[CascadingTypeParameter( nameof( TDto ) )]
public partial class CreateEditPage<TEntity, TDto> : ComponentBase
    where TEntity : IEntity
    where TDto : BaseDto
{
    [Parameter] 
    public RenderFragment ChildContent { get; set; }
    [Parameter] public string CustomSelect { get; set; }

    private TDto _model { get; set; }

    [Parameter]
    public TDto Model { 
        get => _model; 
        set 
        {
            _model = value;
            //ModelChanged.InvokeAsync(value);
        }
    }   

    [Parameter]
    public EventCallback<TDto> ModelChanged { get; set; }

    [Parameter]
    public string Id { get; set; }

    [Parameter]
    public Func<TDto, Task<TDto>> OnInitialized { get; set; }

    [Inject]
    public ICrudService<TEntity, TDto> Service { get; set; }

    [Inject]
    public PageHistoryState PageHistorysState { get; set; }

    [Inject] 
    INotificationService NotificationService { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Inject]
    public PermissionService PermissionService { get; set; }


    public bool IsView => !NavigationManager.Uri.Contains("/edit") && !NavigationManager.Uri.Contains("/new");
    public bool DisabledToEdit => IsView || !PermissionService.CanWrite;
    public string Errors { get; set; }

    protected Validations ValidationsRef { get; set; }
    public bool IsLoading { get; set; }

    protected async override Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(Id))
        {
            base.InvokeAsync(async () =>
            {
                IsLoading = true;
                StateHasChanged();
                var result = await Service.GetById(Id, CustomSelect);
                if (result.Success)
                    Model = result.Data.DeepCloneTo(Model);
                IsLoading = false;
                StateHasChanged();
            });
        }

        if (OnInitialized is not null)
            Model = (await OnInitialized(Model)).DeepCloneTo(Model);
    }

    public async Task OnSubmit()
    {
        IsLoading = true;
        if (ValidationsRef is not null && await ValidationsRef.ValidateAll())
        {
            Errors = null;
            try
            {
                Result<TDto> result = null;
                if (string.IsNullOrEmpty(Id))
                    result = await Service.Create(Model);
                else
                    result = await Service.Update(Id, Model);

                if (result.Success)
                {
                    await NotificationService.Success("Cadastrado com sucesso!", "Resultado");
                    GoBack();
                }

            }
            catch (Exception ex)
            {
                Errors = ex.Message;
                await NotificationService.Error(Errors);
            }
            finally { 
                IsLoading = false; 
            }
        }
        IsLoading = false;
    }

    private void KeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Esc")
            GoBack();
    }

    protected void GoBack() => NavigationManager.NavigateTo(NavigationManager.ToBaseRelativePath(NavigationManager.Uri).Split("/")[0]);

    public void ToEdit()
        => NavigationManager.NavigateTo($"{NavigationManager.Uri}/edit");   
}

