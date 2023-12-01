using Microsoft.AspNetCore.Builder;

namespace CruderSimple.Core.Requests.Base;

public interface IHttpRequestHandler
{
    WebApplication AddEndpointDefinition(WebApplication app);
}