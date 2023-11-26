using CruderSimple.Core.Requests;
using CruderSimple.Core.ViewModels;
using CruderSimple.DynamoDb.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.DynamoDb.Requests;

public static class DeleteRequest 
{
    public record Query([FromRoute] string id) : IEndpointQuery;

    public class Handler<TQuery, TEntity, IOutputDto, IRepository>
        (IRepository repository)
        : HttpHandlerBase<TQuery, TEntity>, IRequestHandler<TQuery, IResult> 
        where TQuery : Query
        where TEntity : Entity
        where IOutputDto : OutputDto 
        where IRepository : Interfaces.IRepository<TEntity>
    {
        public override async Task<IResult> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var entity = await repository.FindById(request.id);
            
            if (entity is null)
                return Results.NotFound();
            await repository.Remove(entity);
            return Results.Ok(entity.ToOutput());
        }
    }
}
