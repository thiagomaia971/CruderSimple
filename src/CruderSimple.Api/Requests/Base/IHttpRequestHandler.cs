using CruderSimple.Core.Interfaces;
using Microsoft.AspNetCore.Builder;

namespace CruderSimple.Api.Requests.Base;

public interface IHttpRequestHandler
{
    WebApplication AddEndpointDefinition<TUser>(WebApplication app)
        where TUser : IUser;
}