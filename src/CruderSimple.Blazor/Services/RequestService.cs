using System.Net.Http.Json;
using System.Text;
using Blazorise;
using CruderSimple.Blazor.Extensions;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Newtonsoft.Json;

namespace CruderSimple.Blazor.Services
{
    public class RequestService
        (IHttpClientFactory httpClientFactory,
        INotificationService notificationService)
        : IRequestService
    {
        public HttpClient HttpClient { get; set; }

        public void HttpClientName(string name)
        {
            HttpClient = httpClientFactory.CreateHttpClient(name);
        }

        public async Task<Result<TDto>> Create<TEntity, TDto>(TDto entity)
            where TEntity : IEntity
            where TDto : BaseDto
        {
            Console.WriteLine(JsonConvert.SerializeObject(entity));
            var result = await HttpClient.PostAsJsonAsync($"v1/{typeof(TEntity).Name}", entity);
            return await result.Content.ReadFromJsonAsync<Result<TDto>>();

        }

        public async Task<Result<TDto>> Delete<TEntity, TDto>(string id)
            where TEntity : IEntity
            where TDto : BaseDto
        {
            var result = await HttpClient.DeleteFromJsonAsync<Result<TDto>>($"v1/{typeof(TEntity).Name}/{id}");
            return result;
        }

        public async Task<Pagination<TDto>> GetAll<TEntity, TDto>(GetAllEndpointQuery query)
            where TEntity : IEntity
            where TDto : BaseDto
        {
            var url = new StringBuilder($"v1/{typeof(TEntity).Name}");

            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(query.select))
                queryString.Add("select", query.select);
            if (query.page > 0)
                queryString.Add("page", query.page.ToString());
            if (query.size > 0)
                queryString.Add("size", query.size.ToString());
            if (!string.IsNullOrEmpty(query.filter))
                queryString.Add("filter", query.filter);
            if (!string.IsNullOrEmpty(query.orderBy))
                queryString.Add("orderBy", query.orderBy);

            if (queryString.Count > 0)
                url.Append($"?{queryString.ToString()}");

            var result = await HttpClient.GetFromJsonAsync<Pagination<TDto>>(url.ToString());
            return result;
        }

        public async Task<Result<TDto>> GetById<TEntity, TDto>(string id)
            where TEntity : IEntity
        {
            return await HttpClient.GetFromJsonAsync<Result<TDto>>($"v1/{typeof(TEntity).Name}/{id}");
        }

        public async Task<Result<TDto>> Update<TEntity, TDto>(string id, TDto entity)
            where TEntity : IEntity
            where TDto : BaseDto
        {
            Console.WriteLine(JsonConvert.SerializeObject(entity));
            var result = await HttpClient.PutAsJsonAsync($"v1/{typeof(TEntity).Name}/{id}", entity);
            try
            {
                var resultContent = await result.Content.ReadFromJsonAsync<Result<TDto>>();
                if (result.IsSuccessStatusCode)
                    return resultContent;

                await notificationService.Error(string.Join(",", resultContent.Errors));
                Console.WriteLine(resultContent.StackTrace);
                return resultContent;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
