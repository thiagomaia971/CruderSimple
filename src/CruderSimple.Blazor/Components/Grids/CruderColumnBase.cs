using Blazorise.DataGrid;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Blazor.Services;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids
{
    [CascadingTypeParameter(nameof(TItem))]
    [CascadingTypeParameter(nameof(TDto))]
    public class CruderColumnBase<TItem, TDto> : ComponentBase
        where TItem : IEntity
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
        [Parameter] public RenderFragment<TItem> DisplayTemplate { get; set; }

        [CascadingParameter] public DataGrid<TItem> DataGridRef { get; set; }
        
        public DataGridSelectColumn<TItem> DataGridSelectColumn { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        protected override Task OnInitializedAsync()
        {
            Attributes.Add("Select", Select);
            return base.OnInitializedAsync();
        }
    }
}
