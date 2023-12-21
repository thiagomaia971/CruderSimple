using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CruderSimple.Blazor.Components
{
    public class BasePageComponent: ComponentBase
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        protected async Task GoBack() 
            => await JSRuntime.InvokeVoidAsync("history.back");
    }
}