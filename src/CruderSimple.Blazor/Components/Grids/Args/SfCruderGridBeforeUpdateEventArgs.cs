using CruderSimple.Core.ViewModels;

namespace CruderSimple.Blazor.Components.Grids.Args
{
    public record SfCruderGridBeforeUpdateEventArgs<TGridDto>(TGridDto Old, TGridDto New)
        where TGridDto: BaseDto;
}
