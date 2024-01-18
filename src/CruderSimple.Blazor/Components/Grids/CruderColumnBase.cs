using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids
{
    [CascadingTypeParameter(nameof(TEntity))]
    [CascadingTypeParameter(nameof(TDto))]
    public class CruderColumnBase<TEntity, TDto> : ComponentBase
        where TEntity : IEntity
        where TDto : BaseDto
    {
        /// <summary>
        /// Field used to Grid
        /// </summary>
        [Parameter] public string ColumnField { get; set; }

        /// <summary>
        /// Caption used to Grid
        /// </summary>
        [Parameter] public string ColumnCaption { get; set; }

        /// <summary>
        /// Property used to sort on Grid component
        /// </summary>
        [Parameter] public string GridSort { get; set; }

        /// <summary>
        /// Custom select request params
        /// </summary>
        [Parameter] public string Select { get; set; }

        /// <summary>
        /// Column is Editable
        /// </summary>
        [Parameter] public bool Editable { get; set; } = true;

        /// <summary>
        /// Column is Filterable
        /// </summary>
        [Parameter] public bool Filterable { get; set; } = true;

        /// <summary>
        /// Column is Sortable
        /// </summary>
        [Parameter] public bool Sortable { get; set; } = true;

        /// <summary>
        /// Render a custom Display Grid
        /// </summary>
        [Parameter] public RenderFragment<TDto> DisplayTemplate { get; set; }

        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        protected override Task OnInitializedAsync()
        {
            Attributes.Add("Select", Select);
            return base.OnInitializedAsync();
        }
    }
}
