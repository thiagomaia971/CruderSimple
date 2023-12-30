using Microsoft.JSInterop;

namespace CruderSimple.Blazor.Services
{
    public class DimensionService(IJSRuntime _js)
    {
        public async Task<ScreenDimension> GetDimensions() 
            => await _js.InvokeAsync<ScreenDimension>("getDimensions");
    }

    public class ScreenDimension
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
