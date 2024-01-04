using CruderSimple.Core.ViewModels;

namespace CruderSimple.Core.Entities;

public class Pagination<T> : Result<IEnumerable<T>>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public string ResultType { get; set; }
    public string Next { get; set; }

    public static Pagination<T> CreateSuccess(int page, int size, IQueryable<T> data)
    {
        return new Pagination<T>
        {
            Success = true,
            Page = page,
            Size = size,
            Data = data
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

public class Pagination : Result
{
    public int Page { get; set; }
    public int Size { get; set; }
    public string ResultType { get; set; }
    public string Next { get; set; }

    public static Pagination CreateSuccess(int page, int size, object data)
    {
        return new Pagination
        {
            Success = true,
            HttpStatusCode = 200,
            Page = page,
            Size = size,
            Data = data
        };
    }

    public static Pagination CreateError(params string[] errors)
    {
        return new Pagination
        {
            Success = false,
            HttpStatusCode = 500,
            Errors = errors.ToList()
        };
    }
}