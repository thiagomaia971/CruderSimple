using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;

namespace CruderSimple.Blazor.Sf.Interfaces.Services
{
    public interface ICrudService<TItem, TDto>
        where TItem : IEntity
        where TDto : BaseDto
    {
        public Task<HttpClient> CreateHttpClient();
        public Task<Pagination<TDto>> GetAll(GetAllEndpointQuery query);
        public Task<ResultViewModel<TDto>> GetById(string id, string select = "*");
        public Task<ResultViewModel<TDto>> Create(TDto entity);
        public Task<ResultViewModel<TDto>> Update(string id, TDto entity);
        public Task<ResultViewModel<TDto>> Delete(string id);
    }

}
