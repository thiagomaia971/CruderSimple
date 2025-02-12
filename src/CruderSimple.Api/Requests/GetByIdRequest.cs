using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Requests;

public static class GetByIdRequest
{
    public record Query([FromRoute] string id, [FromQuery] string select) : IEndpointQuery;

    public class Handler<TQuery, TEntity, TDto, TRepository>
        (TRepository repository)
        : HttpHandlerBase<TQuery, TEntity, Result>, IRequestHandler<TQuery, Result> 
        where TQuery : Query
        where TEntity : IEntity
        where TDto : BaseDto 
        where TRepository : Core.Interfaces.IRepositoryBase<TEntity>
    {
        public override async Task<Result> Handle(TQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var single = await FindById(request.id, request.select, true);
                if (single is null)
                    return Result.CreateError("Recurso não encontrado", 404, "Recurso não encontrado");

                //var outputDto = single.Adapt<TDto>();
                var outputDto = single.ToOutput<TDto>();
                return Result.CreateSuccess(outputDto);
            }
            catch (Exception exception)
            {
                return Result.CreateError(exception.StackTrace, 500, exception.Message);
            }
        }

        public virtual Task<TEntity> FindById(string id, string select = "*", bool asNoTracking = false) 
            => repository.FindById(id, select, asNoTracking);
    }
}