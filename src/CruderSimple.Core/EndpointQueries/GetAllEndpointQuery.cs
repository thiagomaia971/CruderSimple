namespace CruderSimple.Core.EndpointQueries
{

    public record GetAllEndpointQuery(string select, int size, int page) : IEndpointQuery;
}
