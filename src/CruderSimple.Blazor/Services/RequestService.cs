using System.Net.Http.Json;
using System.Text;
using Blazorise;
using Blazorise.DataGrid;
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
        INotificationService notificationService,
        IdentityAuthenticationStateProvider identityAuthenticationStateProvider)
        : IRequestService
    {
        private string _name;
        public HttpClient HttpClient { get; set; }

        public void HttpClientName(string name)
        {
            _name = name;
        }

        public async Task<Result<TDto>> Create<TEntity, TDto>(TDto entity, string url = "")
            where TEntity : IEntity
            where TDto : BaseDto
        {
            return await HandleRequest(async () =>
            {
                await CreateHttpClient();
                Console.WriteLine(JsonConvert.SerializeObject(entity));
                var result = await HttpClient.PostAsJsonAsync($"v1/{typeof(TEntity).Name}/{url}", entity);
                return await result.Content.ReadFromJsonAsync<Result<TDto>>();
            });
        }

        public async Task<Result<TDto>> Delete<TEntity, TDto>(string id, string url = "")
            where TEntity : IEntity
            where TDto : BaseDto
        {
            try
            {

                await CreateHttpClient();
                var result = await HttpClient.DeleteFromJsonAsync<Result<TDto>>($"v1/{typeof(TEntity).Name}/{id}/{url}");
                if (result.Success) return result;

                await notificationService.Error(string.Join(",", result.Errors));
                Console.WriteLine(result.StackTrace);
                return result;
            }
            catch (Exception e)
            {
                await notificationService.Error(string.Join(",", e.Message));
                throw e;
            }
        }

        public async Task<Pagination<TDto>> GetAll<TEntity, TDto>(GetAllEndpointQuery query, string url = "")
            where TEntity : IEntity
            where TDto : BaseDto
        {
            try
            {
                await CreateHttpClient();
                var _url = new StringBuilder($"v1/{typeof(TEntity).Name}/{url}");

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

                var result = await HttpClient.GetFromJsonAsync<Pagination<TDto>>(_url.ToString());
                return result;
            }
            catch (Exception e)
            {
                await notificationService.Error(string.Join(",", e.Message));
                throw e;
            }
        }

        public async Task<Result<TDto>> GetById<TEntity, TDto>(string id, string url = "", string select = "*")
            where TEntity : IEntity
        {
            await CreateHttpClient();
            var _url = new StringBuilder($"v1/{typeof(TEntity).Name}/{id}/{url}");
            if (!string.IsNullOrEmpty(select))
            {
                var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
                queryString.Add("select", select);
                _url.Append($"?{queryString}");
            }
            return await HttpClient.GetFromJsonAsync<Result<TDto>>(_url.ToString());
        }

        public async Task<Result<TDto>> Update<TEntity, TDto>(string id, TDto entity, string url = "")
            where TEntity : IEntity
            where TDto : BaseDto
        {
            await CreateHttpClient();
            Console.WriteLine(JsonConvert.SerializeObject(entity));
            var result = await HttpClient.PutAsJsonAsync($"v1/{typeof(TEntity).Name}/{id}/{url}", entity);
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
                await notificationService.Error(string.Join(",", e.Message));
                throw e;
            }
        }

        private async Task<Result<TDto>> HandleRequest<TDto>(Func<Task<Result<TDto>>> action) where TDto : BaseDto
        {
            Result<TDto> result = null;
            try
            {
                result = await action();
            }
            catch (Exception e)
            {
                await notificationService.Error(string.Join(",", e.Message));
                result = Result<TDto>.CreateError(e.StackTrace, e.Message);
            }

            return result;
        }

        private async Task CreateHttpClient()
        {
            if (HttpClient is null)
            {
                var state = await identityAuthenticationStateProvider.GetAuthenticationStateAsync();
                var loginResult = await identityAuthenticationStateProvider.GetUserInfo();
                HttpClient = httpClientFactory.CreateHttpClient(_name, loginResult.Token);

                foreach (var claim in state.User.Claims)
                    HttpClient.DefaultRequestHeaders.Add(claim.Type.Split("/").LastOrDefault(), claim.Value);
            }
        }
    }
}
