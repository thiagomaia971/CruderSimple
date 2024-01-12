using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;
using Microsoft.Extensions.Configuration;

namespace CruderSimple.Blazor.Services
{
    public class CrudService<TItem, TDto>
        : ICrudService<TItem, TDto>
        where TItem : IEntity
        where TDto : BaseDto
    {
        public IRequestService RequestService { get; }

        public CrudService(IRequestService requestService, IConfiguration configuration)
        {
            RequestService = requestService;
            if (string.IsNullOrEmpty(configuration["API_URL"]))
                throw new ArgumentException("API_URL most be set in Environment Variables");
            RequestService.HttpClientName(configuration["API_URL"]);
        }

        public async Task<Pagination<TDto>> GetAll(GetAllEndpointQuery query) 
            => await RequestService.GetAll<TItem, TDto>(query);

        public async Task<Result<TDto>> GetById(string id, string select)
            => await RequestService.GetById<TItem, TDto>(id, string.Empty, select);

        public async Task<Result<TDto>> Create(TDto entity)
            => await RequestService.Create<TItem, TDto>(entity);

        public async Task<Result<TDto>> Update(string id, TDto entity)
            => await RequestService.Update<TItem, TDto>(id, entity);

        public async Task<Result<TDto>> Delete(string id)
            => await RequestService.Delete<TItem, TDto>(id);
    }
}
