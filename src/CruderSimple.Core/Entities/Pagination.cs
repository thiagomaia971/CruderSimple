namespace CruderSimple.Core.Entities;

public class Pagination<T>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public IEnumerable<T> Data { get; set; }
    public string ResultType { get; set; }
    public string Next { get; set; }
}