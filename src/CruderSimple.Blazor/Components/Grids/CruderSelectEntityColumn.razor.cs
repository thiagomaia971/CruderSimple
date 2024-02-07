using Blazorise.DataGrid;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids;

[CascadingTypeParameter(nameof(TColumnEntity))]
[CascadingTypeParameter(nameof(TColumnDto))]
[CascadingTypeParameter(nameof(TSelectEntityDto))]
public partial class CruderSelectEntityColumn<TColumnEntity, TColumnDto, TSelectEntityDto> 
    : CruderColumnBase<TColumnEntity, TColumnDto>
    where TColumnEntity : IEntity
    where TColumnDto : BaseDto
    where TSelectEntityDto : BaseDto
{
    public string _Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// Property used to search on Grid component inside of Filter
    /// </summary>
    [Parameter] public string GridSearchKey { get; set; }

    /// <summary>
    /// Selects properties to search on Select component request inside of Filter
    /// </summary>
    [Parameter] public string SelectSearchKey { get; set; }

    /// <summary>
    /// Custom select request params
    /// </summary>

    [Inject] public ICrudService<TColumnEntity, TSelectEntityDto> CrudService { get; set; }

    private DataGridSelectColumn<TColumnDto> DataGridSelectColumn { get; set; }
    private DataGrid<TColumnDto> DataGrid => DataGridSelectColumn?.ParentDataGrid;
    public TColumnDto CurrentSelect { get; set; }

    public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, RenderFragment> SelectComponents { get; set; } = new Dictionary<string, RenderFragment>();
    private bool Loaded { get; set; }

    protected override Task OnInitializedAsync()
    {
        Logger.Watch("OnInitialized", () =>
        {
            Attributes.Add("Service", CrudService);
            Attributes.Add("SearchKey", SelectSearchKey);
            Attributes.Add("Field", GridSearchKey);
        });
        return base.OnInitializedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        Logger.Watch("OnAfterRender", () =>
        {
            if (DataGridSelectColumn != null && GridSort != null)
                DataGridSelectColumn.SortField = GridSort;

            if (DataGridSelectColumn != null && DataGridSelectColumn.ParentDataGrid != null)
                Events = (CruderGridEvents<TColumnDto>)DataGridSelectColumn.ParentDataGrid.Attributes["Events"];

            //if (SelectComponent == null && AlwaysEditable)
            //{
            //    SelectComponent = CreateSelectComponent();
            //    StateHasChanged();
            //}
            if (!Loaded && DataGrid != null)
                OnDataGridLoaded();
        });
        base.OnAfterRender(firstRender);
    }

    protected void OnDataGridLoaded()
    {
        Logger.Watch("OnDataGridLoaded", () =>
        {
            Loaded = true;
            Events.OnEditMode += () =>
            {
                Logger.Watch("OnEditMode", () =>
                {
                    InvokeAsync(async () =>
                    {
                        CreateSelectComponent(DataGrid.ReadCellEditValue(ColumnField), null);
                        StateHasChanged();
                    });
                });
            };
        });
    }

    private RenderFragment CreateSelectComponent(object value, TColumnDto item = null, bool force = false)
    {
        var _entity = value as TSelectEntityDto;
        if (force && SelectComponents.ContainsKey(_entity.GetKey))
            SelectComponents.Remove(_entity.GetKey);
        if (SelectComponents.ContainsKey(_entity.GetKey))
            return SelectComponents[_entity.GetKey];

        return Logger.Watch("CreateSelectComponent", () =>
        {
            if (DataGridSelectColumn is null)
                return null;

            if (item == null)
                item = default(TColumnDto);

            var service = DataGridSelectColumn.Attributes["Service"];
            if (service is null)
            {
                Console.WriteLine("Service is null");
                return null;
            }
            var entity = service.GetType().GenericTypeArguments[0];
            var entityDto = service.GetType().GenericTypeArguments[1];

            var render = EntityAutocompleteUtils.CreateComponent(
                entity,
                entityDto,
                value,
                async ((string Key, object Value) value) => await SelectChanged(value/*, cellEdit*/),
                false,
                DataGridSelectColumn.Attributes,
                DisabledEditable(item));

            SelectComponents.Add(_entity.GetKey, render);
            StateHasChanged();
            return render;
        });
    }

    public async Task SelectChanged((string Key, object Value) value/*, CellEditContext<TColumnDto> cellEdit*/)
    {
        Logger.Watch("SelectChanged", () => 
        {
            InvokeAsync(async () =>
            {
                OldValue = CurrentSelect.Adapt<TColumnDto>();
                CurrentSelect.SetValueByPropertyName(value.Value, ColumnField);
                NewValue = CurrentSelect;

                DataGrid.UpdateCellEditValue(ColumnField, value.Value);
                await OnBlur();
                CreateSelectComponent(DataGrid.ReadCellEditValue(ColumnField), null, true);
            });
        });
    }

    protected string GetGridName(TColumnDto item)
    { 
        var itemProperties = item.GetType().GetProperty(ColumnField);
        if (itemProperties is null)
            return item.GetValue;
        return (itemProperties.GetValue(item) as TSelectEntityDto)?.GetValue ?? string.Empty;
    }

    protected override async Task OnClick(TColumnDto item)
    {
        CurrentSelect = item;
        await base.OnClick(item);
    }
}
