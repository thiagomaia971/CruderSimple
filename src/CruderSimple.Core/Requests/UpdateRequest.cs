using CruderSimple.Core.Entities;
using CruderSimple.Core.Requests.Base;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Core.Requests;

public static class UpdateRequest
{
    public record Query<TInputDto>([FromRoute] string id, [FromBody] TInputDto payload) : IEndpointQuery;

    public class Handler<TQuery, TEntity, TInputDto, IOutputDto, IRepository>
        (IRepository repository)
        : HttpHandlerBase<TQuery, TEntity>, IRequestHandler<TQuery, IResult> 
        where TQuery : Query<TInputDto>
        where TEntity : IEntity
        where TInputDto : InputDto 
        where IOutputDto : OutputDto 
        where IRepository : Interfaces.IRepositoryBase<TEntity>
    {        
        public override async Task<IResult> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var entity = await repository.FindById(request.id);

            if (entity is null)
                return Results.NotFound();

            var entityToSave = (TEntity) entity.FromInput(request.payload);
            await repository.Add(entityToSave)
                .Save();
            return Results.Ok(entityToSave.ToOutput());
        }
    }
}