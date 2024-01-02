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
        public bool IsView => !IsEdit;
        public bool IsEdit => !IsNew && PageRoute.Contains("edit");

        public string PageRoute => NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

        public string PageName(string page)
        {
            var pageName = string.Empty;
            if (IsNew) 
                pageName += "Novo ";
            if (IsEdit)
                pageName += "Editar ";
            if (IsView)
                pageName += "Visualizar ";

            return pageName + page;
        }
    }
}
