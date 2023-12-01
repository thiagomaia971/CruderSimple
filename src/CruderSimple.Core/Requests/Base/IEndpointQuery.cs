using MediatR;
using Microsoft.AspNetCore.Http;

namespace CruderSimple.Core.Requests.Base;

public interface IEndpointQuery : IRequest<IResult>;