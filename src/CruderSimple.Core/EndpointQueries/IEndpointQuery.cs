using CruderSimple.Core.ViewModels;
using MediatR;
namespace CruderSimple.Core.EndpointQueries
{
    public interface IEndpointQuery : IRequest<ResultViewModel>;
}
