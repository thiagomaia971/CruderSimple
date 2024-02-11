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
        [Parameter] public List<TGridDto> Data { get; set; }
        [Parameter] public EventCallback<List<TGridDto>> DataChanged { get; set; }

        protected CruderGrid<TGridEntity, TGridDto> CruderGrid { get; set; }

        //public override async Task SetParametersAsync(ParameterView parameters)
        //{
        //    if (parameters.TryGetValue<string>(nameof(FilterBy), out var filterBy))
        //        FilterBy = filterBy;
        //    if (parameters.TryGetValue<List<TGridDto>>(nameof(Data), out var data))
        //        Data = data;
        //    if (parameters.TryGetValue<EventCallback<List<TGridDto>>>(nameof(DataChanged), out var dataChanged))
        //        DataChanged = dataChanged;
        //}

        protected void DataHasChanged(List<TGridDto> data)
        {
            Data = data;
            DataChanged.InvokeAsync(data);
        }
    }
}
