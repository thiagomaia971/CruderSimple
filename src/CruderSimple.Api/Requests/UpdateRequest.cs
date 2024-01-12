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
        : HttpHandlerBase<TQuery, TEntity, Result>, IRequestHandler<TQuery, Result> 
        where TQuery : Query<TDto>
        where TEntity : IEntity
        where TDto : BaseDto 
        where TRepository : Core.Interfaces.IRepositoryBase<TEntity>
    {        
        public override async Task<Result> Handle(TQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await repository.FindById(request.id);

                if (entity is null)
                    return Result.CreateError("Recurso não encontrado", 404, "Recurso não encontrado");

                var entityToSave = (TEntity) entity.FromInput(request.payload);
                await repository.Update(entityToSave)
                    .Save();

                return Result.CreateSuccess(entityToSave.ToOutput<TDto>());
            }
            catch (Exception exception)
            {
                return Result.CreateError(exception.StackTrace, 400, exception.Message);
            }
        }
    }
}