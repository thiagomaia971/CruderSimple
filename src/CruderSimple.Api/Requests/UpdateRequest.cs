using System;
using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Requests;

public static class UpdateRequest
{
    public record Query<TInputDto>([FromRoute] string id, [FromBody] TInputDto payload) : IEndpointQuery;

    public class Handler<TQuery, TEntity, TInputDto, IOutputDto, IRepository>
        (IRepository repository)
        : HttpHandlerBase<TQuery, TEntity, Result>, IRequestHandler<TQuery, Result> 
        where TQuery : Query<TInputDto>
        where TEntity : IEntity
        where TInputDto : InputDto 
        where IOutputDto : OutputDto 
        where IRepository : Core.Interfaces.IRepositoryBase<TEntity>
    {        
        public override async Task<Result> Handle(TQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await repository.FindById(request.id);

                if (entity is null)
                    return Result.CreateError("Recurso não encontrado", 404, ["Recurso não encontrado"]);

                var entityToSave = (TEntity) entity.FromInput(request.payload);
                await repository.Update(entityToSave)
                    .Save();

                return Result.CreateSuccess(entityToSave.ToOutput<IOutputDto>());
            }
            catch (Exception exception)
            {
                return Result.CreateError(exception.StackTrace, 400, exception.Message);
            }
        }
    }
}