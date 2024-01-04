using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using MediatR;

namespace CruderSimple.Api.Requests;

public static class GetAllRequest
{
    public class Handler<TQuery, TEntity, TDto, TRepository>
        (TRepository repository)
        : HttpHandlerBase<TQuery, TEntity, Result>, IRequestHandler<TQuery, Result> 
        where TQuery : GetAllEndpointQuery
        where TEntity : IEntity
        where TDto : BaseDto 
        where TRepository : Core.Interfaces.IRepositoryBase<TEntity>
    {
        public override async Task<Result> Handle(TQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var queryAsync = await repository.GetAll(request);

                return Pagination.CreateSuccess(
                    page: request.page, // TODO
                    size: queryAsync.Size,
                    data: queryAsync.Data.ToOutput<TEntity, TDto>());
            }
            catch (Exception exception)
            {
                return Pagination.CreateError(exception.StackTrace, exception.Message);
            }
        }
    }
}