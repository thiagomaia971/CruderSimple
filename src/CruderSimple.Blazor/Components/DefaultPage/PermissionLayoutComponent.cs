using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.DefaultPage
{
    public abstract class PermissionLayoutComponent : ComponentBase
    {
        [CascadingParameter(Name = "CanRead")]
        public bool CanRead { get; set; }
        [CascadingParameter(Name = "CanWrite")]
        public bool CanWrite { get; set; }
    }
}
