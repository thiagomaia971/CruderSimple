namespace CruderSimple.Core.EndpointQueries
{

    public record GetAllEndpointQuery(
        string select = null, 
        string filter = null, 
        string orderBy = null, 
        int size = 0, 
        int page = 0,
        int skip = 0) : IEndpointQuery;
}
