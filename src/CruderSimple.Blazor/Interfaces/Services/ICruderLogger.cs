namespace CruderSimple.Blazor.Interfaces.Services
{
    public interface ICruderLogger<T>
    {
        void LogDebug(string? message, params object[]? args);
        void LogInformation(string? message, params object[]? args);
        void LogError(string? message, params object[]? args);
        void LogError(Exception? exception, string? message, params object[]? args);
        TResult Watch<TResult>(string stopName, Func<TResult> action);
        Task<TResult> Watch<TResult>(string stopName, Func<Task<TResult>> action);
        Task Watch(string stopName, Func<Task> action);
        void Watch(string stopName, Action action);
    }
}
