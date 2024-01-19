using Blazorise;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter(nameof(TEntity))]
[CascadingTypeParameter(nameof(TItem))]
public partial class CruderDateColumn<TEntity, TItem> : CruderColumnBase<TEntity, TItem>
    where TEntity : IEntity
    where TItem : BaseDto
{
    public DateEdit<DateTime> DatePickerRef { get; set; }
    public DateEdit<DateTime?> DatePickerNullableRef { get; set; }

    public async Task OnFocus(FocusEventArgs e)
    {
        if (DatePickerRef != null)
            await DatePickerRef.ShowPicker();
        if (DatePickerNullableRef != null)
            await DatePickerNullableRef.ShowPicker();
    }
}
