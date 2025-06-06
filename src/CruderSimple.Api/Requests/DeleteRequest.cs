﻿using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Exceptions;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Requests;

public static class DeleteRequest 
{
    public record Query(string? id) : IEndpointQuery;

    public class Handler<TQuery, TEntity, TDto, TRepository>
        (TRepository repository)
        : HttpHandlerBase<TQuery, TEntity, Result>, IRequestHandler<TQuery, Result> 
        where TQuery : Query
        where TEntity : IEntity
        where TDto : BaseDto 
        where TRepository : Core.Interfaces.IRepositoryBase<TEntity>
    {
        public override async Task<Result> Handle(TQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await GetById(request.id);
            
                if (entity is null)
                    return Result.CreateError("Recurso não encontrado", 404, "Recurso não encontrado");

                await repository.Remove(entity)
                    .Save();

                return Result.CreateSuccess(entity.ToOutput<TDto>());
            }
            catch (Exception exception)
            {
                return Result.CreateError(exception.StackTrace, 500, exception.Message);
            }
        }

        public async Task<TEntity> GetById(string id)
        {
            var entity = await repository.FindById(id);

            if (entity is null)
                throw new ResultException(Result.CreateError("Recurso não encontrado", 404, "Recurso não encontrado"));
            return entity;
        }
    }
}
