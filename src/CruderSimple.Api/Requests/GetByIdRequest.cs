using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Requests;

public static class GetByIdRequest
{
    public record Query([FromRoute] string id) : IEndpointQuery;

    public class Handler<TQuery, TEntity, IOutputDto, IRepository>
        (IRepository repository)
        : HttpHandlerBase<TQuery, TEntity, Result>, IRequestHandler<TQuery, Result> 
        where TQuery : Query
        where TEntity : IEntity
        where IOutputDto : OutputDto 
        where IRepository : Core.Interfaces.IRepositoryBase<TEntity>
    {
        public override async Task<Result> Handle(TQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var single = await repository.FindById(request.id);
                if (single is null)
                    return Result.CreateError("Recurso não encontrado", 404, "Recurso não encontrado");

                var outputDto = single.ToOutput<IOutputDto>();
                return Result.CreateSuccess(outputDto);
            }
            catch (Exception exception)
            {
                return Result.CreateError(exception.StackTrace, 500, exception.Message);
            }
        }
    }
}