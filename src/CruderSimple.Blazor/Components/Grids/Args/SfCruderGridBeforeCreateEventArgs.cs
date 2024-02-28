using CruderSimple.Core.ViewModels;

namespace CruderSimple.Blazor.Components.Grids.Args
{
    public record SfCruderGridBeforeCreateEventArgs<TGridDto>(TGridDto Item)
        where TGridDto : BaseDto;
}
