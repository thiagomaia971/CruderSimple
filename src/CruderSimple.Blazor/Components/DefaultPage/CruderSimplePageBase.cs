using Blazorise;
using CruderSimple.Blazor.Services;
using CruderSimple.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics.Metrics;
using static CruderSimple.Blazor.Components.BreadcrumbList;

namespace CruderSimple.Blazor.Components.DefaultPage;

public class CruderSimplePageBase : ComponentBase
{
    [Inject]
    protected PermissionService PermissionService { get; set; }

    [Inject] 
    protected INotificationService NotificationService { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    //[Inject]
    //public PageParameter _pageParameter { get; set; }

    [Inject]
    private IJSRuntime _jSRuntime { get; set; }

    [CascadingParameter]
    public MainPage.WindowDimension Dimension { get; set; }

    public string PageTitle
    {
        get => PageParameter.PageTitle; set
        {
            PageParameter.PageTitle = value;
            //if (PageParameter.PageTitleChanged is not null)
            //    PageParameter.PageTitleChanged();
        }
    }

    public Breadcrum[] Breadcrums { get => PageParameter.Breadcrums; set => PageParameter.Breadcrums = value; }

    public bool CanRead => PermissionService.CanRead;
    public bool CanWrite => PermissionService.CanWrite;
    
    public bool IsNew => NavigationManager.Uri.Contains("/new");
    public bool IsEdit => NavigationManager.Uri.Contains("/edit");
    public bool IsView => !IsNew && !IsEdit;
    public bool IsMobile { get;set;}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            IsMobile = await _jSRuntime.InvokeAsync<bool>("isDevice");
    }

}