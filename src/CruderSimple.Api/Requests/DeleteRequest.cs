using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Requests;

public static class DeleteRequest 
{
    public record Query([FromRoute] string id) : IEndpointQuery;

    public class Handler<TQuery, TEntity, IOutputDto, IRepository>
        (IRepository repository)
        : HttpHandlerBase<TQuery, TEntity>, IRequestHandler<TQuery, IResult> 
        where TQuery : Query
        where TEntity : IEntity
        where IOutputDto : OutputDto 
        where IRepository : Core.Interfaces.IRepositoryBase<TEntity>
    {
        public override async Task<IResult> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var entity = await repository.FindById(request.id);
            
            if (entity is null)
                return Results.NotFound();
            await repository.Remove(entity)
                .Save();
            return Results.Ok(entity.ToOutput<IOutputDto>());
        }
    }
}
