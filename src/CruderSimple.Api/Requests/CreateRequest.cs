using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Interfaces;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Requests;

public static class CreateRequest 
{   
    public record Query<TInputDto>([FromBody] TInputDto payload) : IEndpointQuery where TInputDto : InputDto;
    
    public class Handler<TQuery, TEntity, TInputDto, IOutputDto, IRepository>
        (IRepository repository)
        : HttpHandlerBase<TQuery, TEntity, Result>, IRequestHandler<TQuery, Result> 
        where TQuery : Query<TInputDto>
        where TEntity : IEntity
        where TInputDto : InputDto 
        where IOutputDto : OutputDto 
        where IRepository : IRepositoryBase<TEntity>
    {
        public override async Task<Result> Handle(TQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = (TEntity) Activator.CreateInstance<TEntity>().FromInput(request.payload);
                if (!string.IsNullOrEmpty(entity.GetPrimaryKey()))
                {
                    var entityExist = await repository.FindBy("PrimaryKey", entity.GetPrimaryKey());
            
                    if (entityExist is not null)
                        return Result.CreateSuccess(entityExist.ToOutput<IOutputDto>(), 201);
                }

                await repository.Add(entity)
                    .Save();

                return Result.CreateSuccess(entity.ToOutput<IOutputDto>());
            }
            catch (Exception exception)
            {
                return Result.CreateError(exception.StackTrace, 500, exception.Message);
            }
        }
    }
}