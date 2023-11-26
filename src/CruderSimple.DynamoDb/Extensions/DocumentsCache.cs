namespace CruderSimple.DynamoDb.Extensions;

public static class DocumentsCache
{
    public static IDictionary<string, Type> Types = new Dictionary<string, Type>();
    
    public static Type GetAssemblyBy(this AppDomain appDomain, string typeName)
    {
        if (Types.TryGetValue(typeName, out var by))
            return by;
        
        var type = appDomain
            .GetAssemblies()
            .FirstOrDefault(x => x.GetType(typeName) != null)
            .GetType(typeName);
        Types.Add(typeName, type);
        return type;
    }
}