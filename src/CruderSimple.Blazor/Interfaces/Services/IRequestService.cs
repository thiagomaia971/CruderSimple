using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;

namespace CruderSimple.Blazor.Interfaces.Services
{
    public interface IRequestService
    {
        void HttpClientName(string name);
     
        public Task<Pagination<TDto>> GetAll<TEntity, TDto> (GetAllEndpointQuery query)
            where TEntity : IEntity
            where TDto : BaseDto;

        public Task<Result<TDto>> GetById<TEntity, TDto>(string id)
            where TEntity : IEntity;

        public Task<Result<TDto>> Create<TEntity, TDto> (TDto entity)
            where TEntity : IEntity
            where TDto : BaseDto;

        public Task<Result<TDto>> Update<TEntity, TDto>(string id, TDto entity) 
            where TEntity : IEntity 
            where TDto : BaseDto;

        public Task<Result<TDto>> Delete<TEntity, TDto>(string id) 
            where TEntity : IEntity 
            where TDto : BaseDto;
    }
}
