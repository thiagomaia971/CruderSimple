using CruderSimple.Blazor.Components.Grids;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor;
using System.Text;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.Extensions;
using System.Dynamic;

namespace CruderSimple.Blazor.Adaptors
{
    public record CruderGridApatorParameters<TItem, TDto>(
        DataManager DataManager,
        ICrudService<TItem, TDto> CrudService,
        SfGrid<TDto> Grid,
        string FilterBy)
            where TItem : IEntity
            where TDto : BaseDto;

    public class CruderGridAdaptor<TItem, TDto>(CruderGridApatorParameters<TItem, TDto> parameters)
        : UrlAdaptor(parameters.DataManager)
            where TItem : IEntity
            where TDto : BaseDto
    {
        private DataManagerRequest Queries { get; set; }

        public override object ProcessQuery(DataManagerRequest queries)
        {
            Queries = queries;
            return new RequestOptions
            {
                Url = base.DataManager.Url,
                RequestMethod = HttpMethod.Get,
                BaseUrl = base.DataManager.BaseUri,
                Data = queries
            };
        }


        public override async Task<object> PerformDataOperation<T>(object queries)
        {
            try
            {
                Console.WriteLine(Queries.ToJson());
                var query = await CreateQuery();
                var url = CreateUrlEndpoint(query);

                var result = await parameters.DataManager.HttpClientInstance.GetAsync(url.ToString());
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }

        public override object Update(DataManager dataManager, string keyField, object data, string tableName = null, Query query = null, object original = null, IDictionary<string, object> updateProperties = null)
        {
            Console.WriteLine("Update");
            return base.Update(dataManager, keyField, data, tableName, query, original, updateProperties);
        }

        public override object Update(DataManager dataManager, string keyField, IDynamicMetaObjectProvider data, string tableName = null, Query query = null, object original = null)
        {
            Console.WriteLine("Update");
            return base.Update(dataManager, keyField, data, tableName, query, original);
        }

        public override Task<object> ProcessResponse<T>(object data, DataManagerRequest queries)
        {
            Console.WriteLine("ProcessResponse");
            return base.ProcessResponse<T>(data, queries);
        }

        public override Task<object> ProcessCrudResponse<T>(object data, DataManagerRequest queries)
        {
            Console.WriteLine("ProcessCrudResponse: "+ data.ToJson());
            return base.ProcessCrudResponse<T>(data, queries);
        }

        private async Task<GetAllEndpointQuery> CreateQuery()
        {
            var columns = await parameters.Grid.GetColumnsAsync();
            var select = GetQuerySelect(columns);
            var filter = GetQueryFilter(parameters.FilterBy);
            var sort = GetQuerySort(columns);

            var take = Queries.Take == 0 ? 10 : Queries.Take;
            return new GetAllEndpointQuery(
                select,
                filter,
                sort,
                take,
                (Queries.Skip / take) + 1);
        }

        private string CreateUrlEndpoint(GetAllEndpointQuery query)
        {
            var _url = new StringBuilder($"v1/{typeof(TItem).Name}");

            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(query.select))
                queryString.Add("select", query.select);
            if (query.page != 0)
                queryString.Add("page", query.page.ToString());
            if (query.size != 0)
                queryString.Add("size", query.size.ToString());
            if (query.skip != 0)
                queryString.Add("skip", query.skip.ToString());
            if (!string.IsNullOrEmpty(query.filter))
                queryString.Add("filter", query.filter);
            if (!string.IsNullOrEmpty(query.orderBy))
                queryString.Add("orderBy", query.orderBy);

            if (queryString.Count > 0)
                _url.Append($"?{queryString.ToString()}");

            return _url.ToString();
        }


        protected virtual string GetQuerySelect(List<GridColumn> columns)
        {
            var select = string.Join(",", columns
                .Where(x => !string.IsNullOrEmpty(x.Field))
                .Select(x =>
                {
                    var selectBy = x.CustomAttributes?.ContainsKey(nameof(SfCruderGridColumn<TItem, TDto>.SelectBy)) ?? false ?
                        x.CustomAttributes[nameof(SfCruderGridColumn<TItem, TDto>.SelectBy)] :
                        x.Field;
                    return selectBy;
                })
                .Distinct()
                .ToList());

            return select;
        }

        protected virtual string GetQueryFilter(string filterBy)
        {
            var filters = new List<string> { filterBy };

            foreach (var where in Queries?.Where ?? new List<WhereFilter>())
            {
                foreach (var predicate in where?.predicates ?? new List<WhereFilter>())
                {
                    var filter = $"{predicate.Field} {predicate.Operator.ToOperation()} {predicate.value}";
                    filters.Add(filter);
                    Console.WriteLine(predicate.Operator);
                    Console.WriteLine(filter);
                }
            }
            return string.Join(",", filters);
        }


        protected virtual string GetQuerySort(List<GridColumn> columns)
        {
            var sorters = new List<string>();
            foreach (var sorted in Queries?.Sorted ?? new List<Sort>())
            {
                var column = columns.FirstOrDefault(x => x.Field == sorted.Name);
                var sortBy = column.CustomAttributes?.ContainsKey(nameof(SfCruderGridColumn<TItem, TDto>.SortBy)) ?? false ?
                    column.CustomAttributes[nameof(SfCruderGridColumn<TItem, TDto>.SortBy)] :
                    sorted.Name;
                var sort = $"{sortBy} {sorted.Direction.ToSortDirection()}";
                Console.WriteLine(sorted.Direction);
                Console.WriteLine(sort);
                sorters.Add(sort);
            }
            return string.Join(",", sorters);
        }
    }
}
