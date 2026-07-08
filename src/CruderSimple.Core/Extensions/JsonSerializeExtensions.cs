using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CruderSimple.Core.Extensions;

public static class JsonSerializeExtensions
{
    public static JsonSerializerOptions GetDefaultSerializerOptions(this JsonSerializerOptions jsonSerializerOptions)
    {
        // Callers sometimes pass JsonSerializerOptions.Default (a shared, process-wide
        // singleton). Mutating it in place throws once the runtime has marked it
        // read-only after first use elsewhere - copy into a fresh instance instead.
        var options = jsonSerializerOptions == null
            ? new JsonSerializerOptions()
            : new JsonSerializerOptions(jsonSerializerOptions);

        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.WriteIndented = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.IgnoreReadOnlyFields = true;
        options.IgnoreReadOnlyProperties = true;
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public static Newtonsoft.Json.JsonSerializerSettings GetDefaultSerializerSettings(
        this Newtonsoft.Json.JsonSerializerSettings jsonSerializerOptions)
    {
        
        if (jsonSerializerOptions == null)
            jsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings();

        jsonSerializerOptions.NullValueHandling = NullValueHandling.Include;
        jsonSerializerOptions.DefaultValueHandling = DefaultValueHandling.Include;
        jsonSerializerOptions.Converters.Add(new StringEnumConverter());
        return jsonSerializerOptions;
    }
}