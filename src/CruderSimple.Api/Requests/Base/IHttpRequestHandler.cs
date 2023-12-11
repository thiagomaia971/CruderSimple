using Microsoft.AspNetCore.Builder;

namespace CruderSimple.Api.Requests.Base;

public interface IHttpRequestHandler
{
    WebApplication AddEndpointDefinition(WebApplication app);
}