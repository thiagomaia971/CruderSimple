using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.DefaultPage
{
    public class CreationComponent : PermissionLayoutComponent
    {
        [Parameter]
        public string? Id { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [CascadingParameter(Name = "AppRouteData")]
        public RouteData RouteData { get; set; }

        public bool IsNew => string.IsNullOrEmpty(Id);
        public bool IsEdit => !IsNew;

        public string PageRoute { get; set; }

        public string PageName(string page) 
            => (IsNew ? "Novo " : "Editar ") + page;

        protected override async Task OnInitializedAsync()
        {
            PageRoute = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        }
    }
}
