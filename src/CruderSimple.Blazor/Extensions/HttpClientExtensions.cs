using System.Net.Http.Headers;
using CruderSimple.Blazor.Services;

namespace CruderSimple.Blazor.Extensions
{
    public static class HttpClientExtensions
    {
        public static HttpClient CreateHttpClient(this IHttpClientFactory httpClientFactory, string name) 
        { 
            var httpClient = httpClientFactory.CreateClient(name);
            if (IdentityAuthenticationStateProvider.UserCache is not null)
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IdentityAuthenticationStateProvider.UserCache.Token);
            
            return httpClient;
        }
    }
}
