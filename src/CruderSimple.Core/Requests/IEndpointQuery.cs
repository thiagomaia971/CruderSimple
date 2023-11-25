using MediatR;
using CruderSimple.Core.Requests;
using Microsoft.AspNetCore.Http;

namespace CruderSimple.Core.Requests;

public interface IEndpointQuery : IRequest<IResult>;