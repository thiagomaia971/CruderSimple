namespace CruderSimple.Core.ViewModels
{
    public class ResultViewModel<T>
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
        public string StackTrace { get; set; }
        public T Result { get; set; }

        public static ResultViewModel<T> CreateSuccess(T data)
        {
            return new ResultViewModel<T>
            {
                Success = true,
                Result = data
            };
        }

        public static ResultViewModel<T> CreateError(string stackTrace, params string[] errors)
        {
            return new ResultViewModel<T>
            {
                Success = false,
                Errors = errors.ToList(),
                StackTrace = stackTrace
            };
        }
    }
    public class ResultViewModel
    {
        public int HttpStatusCode { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
        public string StackTrace { get; set; }
        public object Result { get; set; }

        public static ResultViewModel CreateSuccess(object data, int httpStatusCode = 200)
        {
            return new ResultViewModel
            {
                Success = true,
                Result = data,
                HttpStatusCode = httpStatusCode
            };
        }

        public static ResultViewModel CreateError(string stackTrace, int httpStatusCode = 400, params string[] errors)
        {
            return new ResultViewModel
            {
                Success = false,
                HttpStatusCode = httpStatusCode,
                Errors = errors.ToList(),
                StackTrace = stackTrace
            };
        }
    }
}
