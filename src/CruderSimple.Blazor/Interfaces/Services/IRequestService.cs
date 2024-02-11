using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;

namespace CruderSimple.Blazor.Interfaces.Services
{
    public interface IRequestService
    {
        void HttpClientName(string name); 
        public Task<HttpClient> CreateHttpClient();

        public Task<Pagination<TDto>> GetAll<TEntity, TDto> (GetAllEndpointQuery query, string url = "")
            where TEntity : IEntity
            where TDto : BaseDto;

        public Task<ResultViewModel<TDto>> GetById<TEntity, TDto>(string id, string url = "", string select = "*")
            where TEntity : IEntity
            where TDto : BaseDto;

        public Task<ResultViewModel<TDto>> Create<TEntity, TDto> (TDto entity, string url = "")
            where TEntity : IEntity
            where TDto : BaseDto;

        public Task<ResultViewModel<TDto>> Update<TEntity, TDto>(string id, TDto entity, string url = "") 
            where TEntity : IEntity 
            where TDto : BaseDto;

        public Task<ResultViewModel<TDto>> Delete<TEntity, TDto>(string id, string url = "") 
            where TEntity : IEntity 
            where TDto : BaseDto;
    }
}
