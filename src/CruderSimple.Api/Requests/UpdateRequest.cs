﻿using System;
using CruderSimple.Api.Requests.Base;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.Exceptions;
using CruderSimple.Core.Extensions;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CruderSimple.Api.Requests;

public static class UpdateRequest
{
    public record Query<TDto>([FromRoute] string? id, [FromBody] TDto? payload) : IEndpointQuery
        where TDto : BaseDto;

    public class Handler<TQuery, TEntity, TDto, TRepository>
        (TRepository repository)
        : HttpHandlerBase<TQuery, TEntity, Result>, IRequestHandler<TQuery, Result> 
        where TQuery : Query<TDto>
        where TEntity : IEntity
        where TDto : BaseDto 
        where TRepository : Core.Interfaces.IRepositoryBase<TEntity>
    {        
        public override async Task<Result> Handle(TQuery request, CancellationToken cancellationToken)
        {
            try
            {
                
                var (oldEntity,newEntity) = await GetEntities(request.id, request.payload);
                await repository.Update(newEntity)
                    .Save();

                return Result.CreateSuccess((await repository.FindById(request.id)).ToOutput<TDto>());
            }
            catch (ResultException resultException)
            {
                return resultException.Result;
            }
            catch (Exception exception)
            {
                return Result.CreateError(exception.StackTrace, 400, exception.Message);
            }
        }

        public async Task<(TEntity oldEntity, TEntity newEntity)> GetEntities(string id, TDto payload)
        {
            var entity = await GetById(id);
            return (entity, entity == null ? default(TEntity) : (TEntity)entity.FromInput(payload));
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