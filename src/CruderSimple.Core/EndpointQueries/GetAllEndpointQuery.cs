namespace CruderSimple.Core.EndpointQueries
{

    public record GetAllEndpointQuery(
        string select = "*", 
        string filter = "", 
        string orderBy = "", 
        int size = 10, 
        int page = 1,
        int skip = 0) : IEndpointQuery;
}
