using System.Net.Http.Headers;
using CruderSimple.Blazor.Services;

namespace CruderSimple.Blazor.Extensions
{
    public static class HttpClientExtensions
    {
        public static HttpClient CreateHttpClient(this IHttpClientFactory httpClientFactory, string name, string token = null) 
        { 
            var httpClient = httpClientFactory.CreateClient(name);
            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            return httpClient;
        }
    }
}
