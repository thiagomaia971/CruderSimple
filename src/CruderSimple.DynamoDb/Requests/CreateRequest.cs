using CruderSimple.Core.Requests;
using CruderSimple.Core.ViewModels;
using CruderSimple.DynamoDb.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.DynamoDb.Requests;

public static class CreateRequest 
{
    public record Query<TInputDto>([FromBody] TInputDto payload) : IEndpointQuery where TInputDto : InputDto;
    
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
            var entity = (TEntity) Activator.CreateInstance<TEntity>().FromInput(request.payload);
            if (!string.IsNullOrEmpty(entity.PrimaryKey))
            {
                var entityExist = await repository
                    .CreateQuery()
                    .ByGsi(x => x.PrimaryKey, entity.PrimaryKey)
                    .ByInheritedType()
                    .FindAsync();
            
                if (entityExist is not null)
                    return Results.Ok(entityExist.ToOutput());
            }
                
            var outputDto = (await repository.Save(entity)).ToOutput();
            return Results.Ok(outputDto);
        }
    }
}