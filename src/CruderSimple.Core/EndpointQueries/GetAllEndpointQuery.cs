namespace CruderSimple.Core.EndpointQueries
{

    public record GetAllEndpointQuery(string select, string filter, int size, int page) : IEndpointQuery;
}
