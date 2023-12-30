namespace CruderSimple.Core.EndpointQueries
{

    public record GetAllEndpointQuery(string select, string filter, string orderBy, int size, int page) : IEndpointQuery;
}
