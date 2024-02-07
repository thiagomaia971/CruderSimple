using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;

namespace CruderSimple.Blazor.Services
{
    public class CruderLogger<T> (ILogger<T> logger, IWebAssemblyHostEnvironment environment)
    {
        public void LogDebug(string? message, params object[]? args)
        {
            if (!environment.IsProduction())
                logger.LogInformation(message, args);
        }

        public void LogInformation(string? message, params object[]? args)
            => logger.LogInformation(message, args);

        public void LogError(string? message, params object[]? args)
            => logger.LogError(message, args);

        public void LogError(Exception? exception, string? message, params object[]? args)
            => logger.LogError(exception, message, args);


        public TResult Watch<TResult>(string stopName, Func<TResult> action)
        {

            var watch = System.Diagnostics.Stopwatch.StartNew();
            var result = action();
            watch.Stop();
            LogDebug($"{stopName}: {watch.ElapsedMilliseconds}ms");
            return result;
        }

        public async Task<TResult> Watch<TResult>(string stopName, Func<Task<TResult>> action)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var result = await action();
            watch.Stop();
            LogDebug($"{stopName}: {watch.ElapsedMilliseconds}ms");
            return result;
        }

        public async Task Watch(string stopName, Func<Task> action)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            await action();
            watch.Stop();
            LogDebug($"{stopName}: {watch.ElapsedMilliseconds}ms");
        }

        public async void Watch(string stopName, Action action)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            action();
            watch.Stop();
            LogDebug($"{stopName}: {watch.ElapsedMilliseconds}ms");
        }
    }
}
