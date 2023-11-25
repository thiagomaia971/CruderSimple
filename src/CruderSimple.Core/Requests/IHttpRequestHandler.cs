using Microsoft.AspNetCore.Builder;

namespace CruderSimple.Core.Requests;

public interface IHttpRequestHandler
{
    WebApplication AddEndpointDefinition(WebApplication app);
}