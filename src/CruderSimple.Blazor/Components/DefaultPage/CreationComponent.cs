using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.DefaultPage
{
    public class CreationComponent : CruderSimplePageBase
    {
        [Parameter]
        public string? Id { get; set; }

        [CascadingParameter(Name = "AppRouteData")]
        public RouteData RouteData { get; set; }

        public string PageRoute => NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

        public string PageName(string page)
        {
            var pageName = string.Empty;
            if (IsNew) 
                pageName += "Novo ";
            else if (IsEdit)
                pageName += "Editar ";
            else if (IsView)
                pageName += "Visualizar ";

            return pageName + page;
        }
    }
}
