using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;

namespace CruderSimple.Blazor.Interfaces.Services
{
    public interface IRequestService
    {
        void HttpClientName(string name);
     
        public Task<Pagination<TDto>> GetAll<TEntity, TDto> (GetAllEndpointQuery query, string url = "")
            where TEntity : IEntity
            where TDto : BaseDto;

        public Task<Result<TDto>> GetById<TEntity, TDto>(string id, string url = "")
            where TEntity : IEntity;

        public Task<Result<TDto>> Create<TEntity, TDto> (TDto entity, string url = "")
            where TEntity : IEntity
            where TDto : BaseDto;

        public Task<Result<TDto>> Update<TEntity, TDto>(string id, TDto entity, string url = "") 
            where TEntity : IEntity 
            where TDto : BaseDto;

        public Task<Result<TDto>> Delete<TEntity, TDto>(string id, string url = "") 
            where TEntity : IEntity 
            where TDto : BaseDto;
    }
}
