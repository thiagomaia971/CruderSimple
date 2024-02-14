using CruderSimple.Core.ViewModels;

namespace CruderSimple.Blazor.Components.Grids.Args
{
    public record SfCruderGridBeforeDeleteEventArgs<TGridDto>(TGridDto Item)
        where TGridDto: BaseDto;
}
