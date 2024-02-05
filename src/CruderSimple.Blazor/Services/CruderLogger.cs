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
    }
}
