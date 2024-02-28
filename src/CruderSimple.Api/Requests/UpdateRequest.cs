using System;
using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Requests;

public static class UpdateRequest
{
    public record Query<TDto>([FromRoute] string id, [FromBody] TDto payload) : IEndpointQuery
        where TDto : BaseDto;

    public class Handler<TQuery, TEntity, TDto, TRepository>
        (TRepository repository)
        : HttpHandlerBase<TQuery, TEntity, ResultViewModel>, IRequestHandler<TQuery, ResultViewModel> 
        where TQuery : Query<TDto>
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

                var entityToSave = (TEntity) entity.FromInput(request.payload);
                await repository.Update(entityToSave)
                    .Save();

                return ResultViewModel.CreateSuccess((await repository.FindById(request.id)).ToOutput<TDto>());
            }
            catch (Exception exception)
            {
                return ResultViewModel.CreateError(exception.StackTrace, 400, exception.Message);
            }
        }
    }
}