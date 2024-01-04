using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CruderSimple.Blazor.Services
{
    public class PageHistoryState : ComponentBase
    {
        public IJSRuntime JSRuntime { get; set; }

        public PageHistoryState(IJSRuntime jsRuntime)
        {
            JSRuntime = jsRuntime;
        }


        public async Task GoBack()
        {
            await JSRuntime.InvokeVoidAsync("history.back");
        }
    }
}
