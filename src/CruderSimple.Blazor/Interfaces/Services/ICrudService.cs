using CruderSimple.Core.EndpointQueries;
using CruderSimple.Core.Entities;
using CruderSimple.Core.ViewModels;

namespace CruderSimple.Blazor.Interfaces.Services;

public interface ICrudService<TItem, TDto>
    where TItem : IEntity
    where TDto : BaseDto
{
    public Task<Pagination<TDto>> GetAll(GetAllEndpointQuery query);
    public Task<Result<TDto>> GetById(string id);
    public Task<Result<TDto>> Create(TDto entity);
    public Task<Result<TDto>> Update(string id, TDto entity);
    public Task<Result<TDto>> Delete(string id);
}
