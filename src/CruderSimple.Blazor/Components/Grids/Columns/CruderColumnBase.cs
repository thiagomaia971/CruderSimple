using Blazorise.DataGrid;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.Services;
using CruderSimple.Core.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids.Columns
{
    [CascadingTypeParameter(nameof(TColumnEntity))]
    [CascadingTypeParameter(nameof(TColumnDto))]
    public class CruderColumnBase<TColumnEntity, TColumnDto> : ComponentBase
        where TColumnEntity : IEntity
        where TColumnDto : BaseDto
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
        /// Column is Always in Editable mode
        /// </summary>
        [Parameter] public Func<TColumnDto, bool> DisabledEditable { get; set; } = (_) => false;

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
        [Parameter] public RenderFragment<TColumnDto> DisplayTemplate { get; set; }

        /// <summary>
        /// When Type doenst Match, put a tooltip info
        /// </summary>
        [Parameter] public string TooltipValueNull { get; set; }

        /// <summary>
        /// When you want to run this Column without Permissions management
        /// </summary>
        [Parameter] public bool IgnorePermission { get; set; }

        [Inject] protected PermissionService PermissionService { get; set; }

        protected Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
        protected CruderGridEvents<TColumnDto> Events { get; set; }
        protected TColumnDto OldValue { get; set; }
        protected TColumnDto NewValue { get; set; }

        protected override Task OnInitializedAsync()
        {
            Attributes.Add("Select", Select);
            return base.OnInitializedAsync();
        }

        protected async void ValueInlineChanged(TColumnDto item, object value)
        {
            if (OldValue == null)
                OldValue = item.Adapt<TColumnDto>();
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

        protected void ValueChanged(CellEditContext<TColumnDto> cellEdit, double value)
            => cellEdit.UpdateCell(ColumnField, value);

        protected virtual async Task OnClick(TColumnDto item)
        {
            if (!AlwaysEditable)
                Events.RaiseColumnSelected(item);
        }
    }
}
