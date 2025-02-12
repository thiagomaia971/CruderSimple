using CruderSimple.Core.ViewModels;

namespace CruderSimple.Core.Exceptions;

public class ResultException : Exception
{
    public Result Result { get; }

    public ResultException(Result result)
    {
        Result = result;
    }
}