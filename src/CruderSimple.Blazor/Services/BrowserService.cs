using Microsoft.JSInterop;

namespace CruderSimple.Blazor.Services
{
    public class BrowserService
    {
        private IJSRuntime JS = null;
        public event EventHandler<WindowDimension> Resize;
        private int browserWidth;
        private int browserHeight;
        
        public async void Init(IJSRuntime js)
        {
            // enforce single invocation            
            if (JS == null)
            {
                this.JS = js;
                await JS.InvokeAsync<string>("resizeListener", DotNetObjectReference.Create(this));
            }
        }

        public async Task<WindowDimension> Current() 
            => await JS.InvokeAsync<WindowDimension>("getWindowDimensions");

        [JSInvokable]
        public void SetBrowserDimensions(int jsBrowserWidth, int jsBrowserHeight)
        {
            browserWidth = jsBrowserWidth;
            browserHeight = jsBrowserHeight;
            // For simplicity, we're just using the new width
            this.Resize?.Invoke(this, new WindowDimension(jsBrowserWidth, jsBrowserHeight));
        }
    }

    public record WindowDimension(int Widht, int Height);
}
