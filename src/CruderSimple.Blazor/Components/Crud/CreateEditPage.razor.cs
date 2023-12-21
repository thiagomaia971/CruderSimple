using Blazorise;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Blazor.Services;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Force.DeepCloner;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Crud;

[CascadingTypeParameter( nameof( TEntity ) )]
[CascadingTypeParameter( nameof( TDto ) )]
public partial class CreateEditPage<TEntity, TDto> : ComponentBase
    where TEntity : IEntity
    where TDto : BaseDto
{
    [Parameter] 
    public RenderFragment ChildContent { get; set; }

    [Parameter]
    public TDto Model { get; set; }

    [Parameter]
    public string Id { get; set; }


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
            IsLoading = true;
            var result = await Service.GetById(Id);
            if (result.Success)
                result.Data.DeepCloneTo(Model);
            IsLoading = false; 
        }
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
                else
                {
                    await NotificationService.Error(string.Join(",", result.Errors));
                    Console.WriteLine(result.StackTrace);
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

    protected void GoBack() => NavigationManager.NavigateTo(NavigationManager.ToBaseRelativePath(NavigationManager.Uri).Split("/")[0]);

    public void ToEdit()
        => NavigationManager.NavigateTo($"{NavigationManager.Uri}/edit");   
}
