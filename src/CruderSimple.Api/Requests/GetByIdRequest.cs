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
        : HttpHandlerBase<TQuery, TEntity, ResultViewModel>, IRequestHandler<TQuery, ResultViewModel> 
        where TQuery : Query
        where TEntity : IEntity
        where TDto : BaseDto 
        where TRepository : Core.Interfaces.IRepository<TEntity>
    {
        public override async Task<ResultViewModel> Handle(TQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var single = await repository.FindById(request.id, request.select, true);
                if (single is null)
                    return ResultViewModel.CreateError("Recurso não encontrado", 404, "Recurso não encontrado");

                //var outputDto = single.Adapt<TDto>();
                var outputDto = single.ToOutput<TDto>();
                return ResultViewModel.CreateSuccess(outputDto);
            }
            catch (Exception exception)
            {
                return ResultViewModel.CreateError(exception.StackTrace, 500, exception.Message);
            }
        }
    }
}