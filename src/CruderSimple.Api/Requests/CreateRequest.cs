using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.Interfaces;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Requests;

public static class CreateRequest 
{   
    public record Query<TDto>([FromBody] TDto payload) : IEndpointQuery 
        where TDto : BaseDto;
    
    public class Handler<TQuery, TEntity, TDto, TRepository>
        (TRepository repository)
        : HttpHandlerBase<TQuery, TEntity, Result>, IRequestHandler<TQuery, Result> 
        where TQuery : Query<TDto>
        where TEntity : IEntity
        where TDto : BaseDto 
        where TRepository : IRepositoryBase<TEntity>
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
                        return Result.CreateSuccess(entityExist.ToOutput<TDto>(), 201);
                }

                await repository.Add(entity)
                    .Save();

                var findById = await repository.FindById(entity.Id);
                return Result.CreateSuccess(findById.ToOutput<TDto>());
            }
            catch (Exception exception)
            {
                return Result.CreateError(exception.StackTrace, 500, exception.Message);
            }
        }
    }
}