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
    [Parameter] public RenderFragment<TItem> DisplayTemplate { get; set; }
    [Parameter] public string Field { get; set; }
    [Parameter] public string Caption { get; set; }
    [Parameter] public string SearchKey { get; set; }
    [Parameter] public string Select { get; set; }

    [Inject] public ICrudService<TEntity, TDto> CrudService { get; set; }

    [CascadingParameter] public DataGrid<TItem> DataGridRef { get; set; }
    public DataGridSelectColumn<TItem> DataGridSelectColumn { get; set; }

    public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    protected override Task OnInitializedAsync()
    {
        Attributes.Add("Service", CrudService);
        Attributes.Add("SearchKey", SearchKey);
        Attributes.Add("Select", Select);
        //DataGridRef.AddColumn(DataGridSelectColumn);
        return base.OnInitializedAsync();
    }
}
