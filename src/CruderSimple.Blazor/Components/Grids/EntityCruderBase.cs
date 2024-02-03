using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids
{
    public class EntityCruderBase<TGridEntity, TGridDto> : ComponentBase
        where TGridEntity : IEntity
        where TGridDto : BaseDto
    {
        [Parameter, EditorRequired] public string FilterBy { get; set; }
        [Parameter] public IEnumerable<TGridDto> Data { get; set; }
        [Parameter] public EventCallback<IEnumerable<TGridDto>> DataChanged { get; set; }
        protected CruderGrid<TGridEntity, TGridDto> CruderGrid { get; set; }

        protected async Task DataHasChanged(IEnumerable<TGridDto> data)
        {
            Data = data;
            await DataChanged.InvokeAsync(data);
        }
    }
}
