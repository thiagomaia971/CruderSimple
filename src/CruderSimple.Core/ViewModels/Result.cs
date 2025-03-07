namespace CruderSimple.Core.ViewModels
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
        public string StackTrace { get; set; }
        public T Data { get; set; }

        public static Result<T> CreateSuccess(T data)
        {
            return new Result<T>
            {
                Success = true,
                Data = data
            };
        }

        public static Result<T> CreateError(string stackTrace, params string[] errors)
        {
            return new Result<T>
            {
                Success = false,
                Errors = errors.ToList(),
                StackTrace = stackTrace
            };
        }
    }
    public class Result
    {
        public int HttpStatusCode { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
        public string StackTrace { get; set; }
        public object Data { get; set; }

        public static Result CreateSuccess(object data, int httpStatusCode = 200)
        {
            return new Result
            {
                Success = true,
                Data = data,
                HttpStatusCode = httpStatusCode
            };
        }

        public static Result CreateError(string stackTrace, int httpStatusCode = 400, params string[] errors)
        {
            return new Result
            {
                Success = false,
                HttpStatusCode = httpStatusCode,
                Errors = errors.ToList(),
                StackTrace = stackTrace
            };
        }

        public static Result CreateError(string stackTrace, params string[] errors)
        {
            return new Result
            {
                Success = false,
                HttpStatusCode = 400,
                Errors = errors.ToList(),
                StackTrace = stackTrace
            };
        }
    }
}
