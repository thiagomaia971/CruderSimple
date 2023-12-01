using CruderSimple.Core.Entities;
using CruderSimple.Core.Interfaces;
using CruderSimple.Core.Requests.Base;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Core.Requests;

public static class CreateRequest 
{
    public record Query<TInputDto>([FromBody] TInputDto payload) : IEndpointQuery where TInputDto : InputDto;
    
    public class Handler<TQuery, TEntity, TInputDto, IOutputDto, IRepository>
        (IRepository repository)
        : HttpHandlerBase<TQuery, TEntity>, IRequestHandler<TQuery, IResult> 
        where TQuery : Query<TInputDto>
        where TEntity : IEntity
        where TInputDto : InputDto 
        where IOutputDto : OutputDto 
        where IRepository : IRepository<TEntity>
    {
        public override async Task<IResult> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var entity = (TEntity) Activator.CreateInstance<TEntity>().FromInput(request.payload);
            if (!string.IsNullOrEmpty(entity.GetPrimaryKey()))
            {
                var entityExist = await repository.FindBy("PrimaryKey", entity.GetPrimaryKey());
            
                if (entityExist is not null)
                    return Results.Ok(entityExist.ToOutput());
            }

            await repository.Add(entity)
                .Save();
            return Results.Ok(entity.ToOutput());
        }
    }
}