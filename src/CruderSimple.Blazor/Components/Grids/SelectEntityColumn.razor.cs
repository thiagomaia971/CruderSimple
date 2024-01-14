using Blazorise.DataGrid;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter(nameof(TItem))]
[CascadingTypeParameter(nameof(TEntity))]
[CascadingTypeParameter(nameof(TDto))]
public partial class SelectEntityColumn<TItem, TEntity, TDto> : ComponentBase
    where TEntity : IEntity
    where TItem : BaseDto
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
    /// Property used to search on Select component inside of Filter
    /// </summary>
    [Parameter] public string GridSearchKey { get; set; }

    /// <summary>
    /// Selects properties to search on Select component request inside of Filter
    /// </summary>
    [Parameter] public string SelectSearchKey { get; set; }

    /// <summary>
    /// Custom select request params
    /// </summary>
    [Parameter] public string Select { get; set; }

    /// <summary>
    /// Render a custom Display Grid
    /// </summary>
    [Parameter] public RenderFragment<TItem> DisplayTemplate { get; set; }

    [Inject] public ICrudService<TEntity, TDto> CrudService { get; set; }

    [CascadingParameter] public DataGrid<TItem> DataGridRef { get; set; }
    public DataGridSelectColumn<TItem> DataGridSelectColumn { get; set; }

    public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    protected override Task OnInitializedAsync()
    {
        Attributes.Add("Service", CrudService);
        Attributes.Add("SearchKey", SelectSearchKey);
        Attributes.Add("Field", GridSearchKey);
        Attributes.Add("Select", Select);
        //DataGridRef.AddColumn(DataGridSelectColumn);
        return base.OnInitializedAsync();
    }
}
