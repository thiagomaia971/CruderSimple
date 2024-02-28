using CruderSimple.Core.ViewModels;

namespace CruderSimple.Core.Entities;

public class Pagination<T> : ResultViewModel<IEnumerable<T>>
{
    public int Page { get; set; }
    public int Count { get; set; }
    public string ResultType { get; set; }
    public string Next { get; set; }

    public static Pagination<T> CreateSuccess(int page, int size, IQueryable<T> data)
    {
        return new Pagination<T>
        {
            Success = true,
            Page = page,
            Count = size,
            Result = data
        };
    }

    public static Pagination<T> CreateError(params string[] errors)
    {
        return new Pagination<T>
        {
            Success = false,
            Errors = errors.ToList()
        };
    }
}

public class Pagination : ResultViewModel
{
    public int Page { get; set; }
    public int Count { get; set; }
    public string ResultType { get; set; }
    public string Next { get; set; }

    public static Pagination CreateSuccess(int page, int size, object data)
    {
        return new Pagination
        {
            Success = true,
            HttpStatusCode = 200,
            Page = page,
            Count = size,
            Result = data
        };
    }

    public static Pagination CreateError(string stackTrace, params string[] errors)
    {
        return new Pagination
        {
            Success = false,
            HttpStatusCode = 500,
            StackTrace = stackTrace,
            Errors = errors.ToList()
        };
    }
}