using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Requests;

public static class DeleteRequest 
{
    public record Query([FromRoute] string id) : IEndpointQuery;

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
                var entity = await repository.FindById(request.id);
            
                if (entity is null)
                    return ResultViewModel.CreateError("Recurso não encontrado", 404, "Recurso não encontrado");

                await repository.Remove(entity)
                    .Save();

                return ResultViewModel.CreateSuccess(entity.ToOutput<TDto>());
            }
            catch (Exception exception)
            {
                return ResultViewModel.CreateError(exception.StackTrace, 500, exception.Message);
            }
        }
    }
}
