using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CruderSimple.Core.Extensions;

public static class JsonSerializeExtensions
{
    public static JsonSerializerOptions GetDefaultSerializerOptions(this JsonSerializerOptions jsonSerializerOptions)
    {
        if (jsonSerializerOptions == null)
            jsonSerializerOptions = new JsonSerializerOptions();

        
        jsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        jsonSerializerOptions.WriteIndented = true;
        jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        jsonSerializerOptions.IgnoreReadOnlyFields = true;
        jsonSerializerOptions.IgnoreReadOnlyProperties = true;
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        return jsonSerializerOptions;
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