﻿using Blazorise.DataGrid;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components.Grids.Columns;

[CascadingTypeParameter(nameof(TColumnEntity))]
[CascadingTypeParameter(nameof(TColumnDto))]
[CascadingTypeParameter(nameof(TSelectEnum))]
public partial class CruderSelectColumn<TColumnEntity, TColumnDto, TSelectEnum> 
    : CruderColumnBase<TColumnEntity, TColumnDto>
    where TColumnEntity : IEntity
    where TColumnDto : BaseDto
    where TSelectEnum : Enum
{
    /// <summary>
    /// Property used to search on Grid component inside of Filter
    /// </summary>
    [Parameter] public string GridSearchKey { get; set; }

    /// <summary>
    /// Selects properties to search on Select component request inside of Filter
    /// </summary>
    [Parameter] public string SelectSearchKey { get; set; }

    private DataGridSelectColumn<TColumnDto> DataGridSelectColumn { get; set; }
    private DataGrid<TColumnDto> DataGrid => DataGridSelectColumn?.ParentDataGrid;
    public TColumnDto CurrentSelect { get; set; }

    public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    public RenderFragment SelectComponent { get; set; }
    private bool Loaded { get; set; }

    protected override Task OnInitializedAsync()
    {
        Attributes.Add("SearchKey", SelectSearchKey);
        Attributes.Add("Field", GridSearchKey);
        return base.OnInitializedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
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
        base.OnAfterRender(firstRender);
    }

    protected void OnDataGridLoaded()
    {
        Loaded = true;
        Events.OnEditMode += () =>
        {
            SelectComponent = CreateSelectComponent(DataGrid.ReadCellEditValue(ColumnField));
            StateHasChanged();
        };
    }

    private RenderFragment CreateSelectComponent(object value, TColumnDto item = null)
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
        StateHasChanged();
        return render;
    }

    public async Task SelectChanged((string Key, object Value) value/*, CellEditContext<TColumnDto> cellEdit*/)
    {
        OldValue = CurrentSelect.Adapt<TColumnDto>();
        CurrentSelect.SetValueByPropertyName(value.Value, ColumnField);
        NewValue = CurrentSelect;

        DataGrid.UpdateCellEditValue(ColumnField, value.Value);
        await OnBlur();
        SelectComponent = CreateSelectComponent(DataGrid.ReadCellEditValue(ColumnField));
    }

    protected string GetGridName(TColumnDto item)
    { 
        return item.GetType().GetProperty(ColumnField)?.GetValue(item)?.ToString() ?? string.Empty;
    }

    protected override async Task OnClick(TColumnDto item)
    {
        CurrentSelect = item;
        await base.OnClick(item);
    }
}
