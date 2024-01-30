using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.Services;
using CruderSimple.Core.ViewModels;
using Mapster;
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
        /// Column is Always in Editable mode
        /// </summary>
        [Parameter] public bool AlwaysEditable { get; set; } = false;

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

        /// <summary>
        /// When Type doenst Match, put a tooltip info
        /// </summary>
        [Parameter] public string TooltipValueNull { get; set; }

        [Inject] protected PermissionService PermissionService { get; set; }

        protected Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
        protected CruderGridEvents<TDto> Events { get; set; }
        protected TDto OldValue { get; set; }
        protected TDto NewValue { get; set; }

        protected override Task OnInitializedAsync()
        {
            Attributes.Add("Select", Select);
            return base.OnInitializedAsync();
        }

        protected async void ValueInlineChanged(TDto item, object value)
        {
            if (OldValue == null)
                OldValue = item.Adapt<TDto>();
            if (NewValue == null)
                NewValue = item;

            item.SetValueByPropertyName(value, ColumnField);

            await OnBlur();
        }

        protected async Task OnBlur()
        {
            if (OldValue != null && NewValue != null)
                Events.RaiseOnColumnValueChanged(OldValue, NewValue);
            OldValue = null;
            NewValue = null;
        }

        protected void ValueChanged(CellEditContext<TDto> cellEdit, double value)
            => cellEdit.UpdateCell(ColumnField, value);

        protected virtual async Task OnClick(TDto item)
        {
            if (!AlwaysEditable)
                Events.RaiseColumnSelected(item);
        }
    }
}
