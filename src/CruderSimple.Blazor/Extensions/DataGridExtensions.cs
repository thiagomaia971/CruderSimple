using Blazorise;
using Blazorise.DataGrid;
using Blazorise.Extensions;

namespace CruderSimple.Blazor.Extensions
{
    public static class DataGridExtensions
    {
        public static void SetFilterValue<TDto>(this DataGridColumn<TDto> column, params string[] value)
        {
            var searchValueSplited = new string[value.Length + 1];
            for (int i = 0; i < value.Length; i++)
                searchValueSplited[i + 1] = value[i];
            column.Filter.SearchValue = string.Join("_", searchValueSplited);
        }
        public static string GetFilterValue<TDto>(this DataGridColumn<TDto> column)
        {
            var searchValueSplited = column.Filter.SearchValue is null ? new string[3] : column.Filter.SearchValue?.ToString().Split("_");
            string v = string.Join("_", searchValueSplited.Take(1..searchValueSplited.Length));
            return v;
        }

        public static void SetFilterMethod<TDto>(this DataGridColumn<TDto> column, DataGridColumnFilterMethod value)
        {
            var searchValueSplited = column.Filter.SearchValue is null ? new string[3] : column.Filter.SearchValue?.ToString().Split("_");
            searchValueSplited[0] = ((int) value).ToString();
            column.Filter.SearchValue = string.Join("_", searchValueSplited);
        }

        public static DataGridColumnFilterMethod GetFilterMethod<TDto>(this DataGridColumn<TDto> column)
        {
            var searchValueSplited = column.Filter.SearchValue is null ? new string[3] : column.Filter.SearchValue?.ToString().Split("_");
            if (string.IsNullOrEmpty(searchValueSplited[0]))
                return DataGridColumnFilterMethod.Contains;
            return (DataGridColumnFilterMethod) (int.Parse(searchValueSplited[0]));
        }
    }
}
