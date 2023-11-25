using CruderSimple.Core.Requests;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Entity = CruderSimple.DynamoDb.Entities.Entity;

namespace CruderSimple.DynamoDb.Requests;

public static class UpdateRequest
{
    public record Query<TInputDto>([FromRoute] string id, [FromBody] TInputDto payload) : IEndpointQuery;

    public class Handler<TQuery, TEntity, TInputDto, IOutputDto, IRepository>
        (IRepository repository)
        : HttpHandlerBase<TQuery, TEntity>, IRequestHandler<TQuery, IResult> 
        where TQuery : Query<TInputDto>
        where TEntity : Entity
        where TInputDto : InputDto 
        where IOutputDto : OutputDto 
        where IRepository : Interfaces.IRepository<TEntity>
    {        
        public override async Task<IResult> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var entity = await repository.CreateQuery().ById(request.id).FindAsync();

            if (entity is null)
                return Results.NotFound();

            var entityToSave = (TEntity) entity.FromInput(request.payload);
            return Results.Ok((await repository.Save(entityToSave)).ToOutput());
        }
    }
}