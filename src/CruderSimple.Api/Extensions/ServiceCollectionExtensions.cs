using System.Diagnostics;
using CruderSimple.Api.Requests.Base;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using CruderSimple.Core.Extensions;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using CruderSimple.Core.ViewModels;
using Microsoft.AspNetCore.Http;

namespace CruderSimple.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCruderRequestDefinitions(this IServiceCollection services)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            services.AddMemoryCache();
            AddEndpoints(services);
            AddCorsPolicy(services);
            ConfigJsonSerializer(services);
            services.AddPermissionsAuthorization();
            
            stopWatch.Stop();
            Console.WriteLine($"AddRequestDefinitions in: {stopWatch.ElapsedMilliseconds}ms");
            return services;
        }

        private static void AddEndpoints(IServiceCollection services)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.ExportedTypes);
            var requestHandlers = types.GetTypesWithHelpAttribute<EndpointRequest>();

            foreach (var handler in requestHandlers)
            {
                var query = handler.BaseType.GetGenericArguments().First();
                var @interface = typeof(IRequestHandler<,>).MakeGenericType(query, typeof(ResultViewModel));
                services.AddScoped(@interface, handler);
            }
        }

        private static void AddCorsPolicy(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        private static void ConfigJsonSerializer(IServiceCollection services)
        {
            services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.SerializerOptions.WriteIndented = true;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                options.SerializerOptions.IgnoreReadOnlyFields = true;
                options.SerializerOptions.IgnoreReadOnlyProperties = true;
                
            });
        }
    }
}
