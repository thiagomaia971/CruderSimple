﻿using CruderSimple.Core.Entities;
using CruderSimple.Core.Requests;
using CruderSimple.Core.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Entity = CruderSimple.DynamoDb.Entities.Entity;

namespace CruderSimple.DynamoDb.Requests;

public static class GetAllRequest
{
    public record Query : IEndpointQuery;

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
            var queryAsync = await repository.GetAll();
            return Results.Ok(new Pagination<IOutputDto>()
            {
                Page = queryAsync.Page,
                Size = queryAsync.Size,
                Next = queryAsync.Next,
                Data = queryAsync.Data.Select(x => x.ToOutput()).Cast<IOutputDto>()
            });
        }
    }
}