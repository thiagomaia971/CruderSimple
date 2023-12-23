using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;

namespace CruderSimple.Blazor.Services
{
    public class CrudService<TItem, TDto>
        : ICrudService<TItem, TDto>
        where TItem : IEntity
        where TDto : BaseDto
    {
        public IRequestService RequestService { get; }

        public CrudService(IRequestService requestService)
        {
            RequestService = requestService;
            RequestService.HttpClientName("OdontoManagement.Api");
        }

        public async Task<Pagination<TDto>> GetAll(GetAllEndpointQuery query) 
            => await RequestService.GetAll<TItem, TDto>(query);

        public async Task<Result<TDto>> GetById(string id)
            => await RequestService.GetById<TItem, TDto>(id);

        public async Task<Result<TDto>> Create(TDto entity)
            => await RequestService.Create<TItem, TDto>(entity);

        public async Task<Result<TDto>> Update(string id, TDto entity)
            => await RequestService.Update<TItem, TDto>(id, entity);

        public async Task<Result<TDto>> Delete(string id)
            => await RequestService.Delete<TItem, TDto>(id);
    }
}
