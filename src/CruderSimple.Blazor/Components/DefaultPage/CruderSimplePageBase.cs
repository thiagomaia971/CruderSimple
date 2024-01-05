using Blazorise;
using CruderSimple.Blazor.Services;
using CruderSimple.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static CruderSimple.Blazor.Components.BreadcrumbList;

namespace CruderSimple.Blazor.Components.DefaultPage;

public class CruderSimplePageBase : ComponentBase
{
    [Inject]
    private PermissionService _permissionService { get; set; }

    [Inject] 
    protected INotificationService NotificationService { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    [Inject]
    private PageParameter _pageParameter { get; set; }

    [Inject]
    private IJSRuntime _jSRuntime { get; set; }

    public string PageTitle { get => _pageParameter.PageTitle; set => _pageParameter.PageTitle = value; }
    public Breadcrum[] Breadcrums { get => _pageParameter.Breadcrums; set => _pageParameter.Breadcrums = value; }

    public bool CanRead => _permissionService.CanRead;
    public bool CanWrite => _permissionService.CanWrite;
    
    public bool IsNew => NavigationManager.Uri.Contains("/new");
    public bool IsEdit => NavigationManager.Uri.Contains("/edit");
    public bool IsView => !IsNew && !IsEdit;
    public bool IsMobile { get;set;}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            IsMobile = await _jSRuntime.InvokeAsync<bool>("isDevice");
        Console.WriteLine("IsMobile:" + IsMobile);
    }

}