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
        [Parameter] public IList<TGridDto> Data { get; set; }
        [Parameter] public Action<IList<TGridDto>> DataChanged { get; set; }
        protected CruderGrid<TGridEntity, TGridDto> CruderGrid { get; set; }

        protected void DataHasChanged(IList<TGridDto> data)
        {
            Data = data;
            DataChanged(data);
        }
    }
}
