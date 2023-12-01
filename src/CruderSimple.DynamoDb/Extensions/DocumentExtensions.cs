using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CruderSimple.Core.Extensions;
using CruderSimple.DynamoDb.Attributes;
using CruderSimple.DynamoDb.Entities;
using Newtonsoft.Json;

namespace CruderSimple.DynamoDb.Extensions;

public static class DocumentExtensions
{
    public static IEnumerable<T> ToEntities<T>(this IDynamoDBContext dynamoDbContext, List<Document> documents)
        where T : Entity
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var entities = new List<T>();

        var entitiesFromDocuments = documents
            .Select(x =>
            {
                var assemblyBy = AppDomain.CurrentDomain
                    .GetAssemblyBy(x[nameof(Entity.EntityType)]);
                return FromDocumentGeneric(
                    dynamoDbContext,
                    assemblyBy,
                    x);
            })
            .GroupBy(x => (x.InheritedType, x.InheritedKey), x=> x);

        foreach (var entityGrouped in entitiesFromDocuments)
        {
            var type = entityGrouped.Key.InheritedType;
            var entity = entityGrouped.FirstOrDefault(x => x.EntityType == type);
            
            var innerEntities = entityGrouped.GroupBy(x => x.EntityType).Where(x => x.Key != entity.EntityType);

            foreach (var innerGrouped in innerEntities)
            {
                var innerType = AppDomain.CurrentDomain
                    .GetAssemblyBy(innerGrouped.Key);
                var genericList = typeof(List<>).MakeGenericType(innerType);
                var listOfInnerType = Activator.CreateInstance(genericList);

                foreach (var innerEntity in innerGrouped)
                    listOfInnerType.GetType().GetMethod("Add").Invoke(listOfInnerType, new [] { innerEntity });
                
                var listProperty = entity
                    .GetPropertiesWithAttribute<DynamoDbInner>()
                    .FirstOrDefault(x => x.GetCustomAttribute<DynamoDbInner>().Type == innerType);
                
                listProperty.SetValue(entity, listOfInnerType);
            }
            entities.Add((T) entity);
        }
        
        
        /*
        var others = new Dictionary<(string id, string entityType), List<string>>();
        foreach (var document in documents)
        {
            var documentEntityType = document[nameof(Entity.EntityType)].AsString();
            if (documentEntityType == typeof(T).Name)
                entities.Add(dynamoDbContext.FromDocument<T>(document));
            else
            {
                var json = document.ToJson();
                var documentId = document[nameof(Entity.InheritedKey)].AsString();
                if (others.ContainsKey((documentId, documentEntityType)))
                    others[(documentId, documentEntityType)].Add(json);
                else
                    others.Add((documentId, documentEntityType), new List<string>{ json });
            }
        }

        if (others.Count == 0)
            return entities;

        foreach (var entity in entities)
        {
            var properties = entity.GetPropertiesWithAttribute<DynamoDbInner>();

            foreach (var propertyInfo in properties)
            {
                var dynamoDbInner = propertyInfo.GetCustomAttribute<DynamoDbInner>();
                var type = dynamoDbInner.Type;
                var list = others[($"{entity.Id}:{typeof(T).Name}", type.Name)].Select(x => JsonConvert.DeserializeObject(x, type));

                var makeGenericType = typeof(List<>).MakeGenericType(type);
                var instance = Activator.CreateInstance(makeGenericType);
                foreach (var o in list)
                {
                    instance.GetType().GetMethod("Add").Invoke(instance, new []{o});
                    propertyInfo.SetValue(entity,   instance);
                }
            }
        }*/
        stopWatch.Stop();
        Console.WriteLine($"Stops in: {stopWatch.ElapsedMilliseconds}ms");
        return entities;
    }

    private static Entity FromDocumentGeneric(IDynamoDBContext dynamoDbContext, Type innerType, Document innerDocument)
    {
        dynamoDbContext.FromDocument<Document>(innerDocument);
        var methodInfo = dynamoDbContext.GetType().GetMethod("FromDocument", new[] { typeof(Document) });
        var methodGenericInfo =
            methodInfo.MakeGenericMethod(innerType);

        var innerEntity = (Entity)methodGenericInfo.Invoke(dynamoDbContext, new[] { innerDocument });
        return innerEntity;
    }

    public static List<Entity> SegregateEntities(this Entity entity)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        
        var entities = new List<Entity>{entity};

        if (string.IsNullOrEmpty(entity.Id))
            entity.Id = Guid.NewGuid().ToString();
        if (string.IsNullOrEmpty(entity.InheritedType))
            entity.InheritedType = entity.GetType().FullName;
        if (string.IsNullOrEmpty(entity.PrimaryKey))
            entity.PrimaryKey = entity.Id;
        if (string.IsNullOrEmpty(entity.InheritedKey))
            entity.InheritedKey = entity.Id;

        var properties = entity.GetPropertiesWithAttribute<DynamoDbInner>();
        
        foreach (var propertyInfo in properties)
        {
            var list = propertyInfo.GetValue(entity, null) as IList;
            
            foreach (var inner in list)
            {
                var innerEntity = (Entity) inner;
                innerEntity.InheritedKey = entity.Id;
                innerEntity.InheritedType = entity.GetType().FullName;
                var innerSegregateEntities = innerEntity.SegregateEntities();

                entities.GetType().GetMethod("AddRange").Invoke(entities, new[] { innerSegregateEntities });
            }
        }

        stopWatch.Stop();
        Console.WriteLine($"Stops in: {stopWatch.ElapsedMilliseconds}ms");
        return entities;
    }

    public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T>(this object @object)
        where T : Attribute 
        => @object.GetType()
            .GetProperties()
            .Where(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(T)))
            .ToList();
    
    public static IEnumerable<PropertyInfo> GetPropertiesWithoutAttribute<T>(this object @object)
        where T : Attribute 
        => @object.GetType()
            .GetProperties()
            .Where(x => x.CustomAttributes.All(x => x.AttributeType != typeof(T)))
            .ToList();
    

    public static T GetPropertyWithAttribute<T>(this object @object)
        where T : Attribute
        => ((T?) @object.GetType().GetCustomAttributes(true).FirstOrDefault(x => typeof(T).IsAssignableFrom(x.GetType())));

    public static T? GetCustomAttribute<T>(this PropertyInfo property)
        where T : Attribute 
        => (T?) property.GetCustomAttributes(false)
            .FirstOrDefault(y => typeof(T).IsAssignableFrom(y.GetType()));

    public static string GetCollumnName(this PropertyInfo property)
    {
        var collumnName = property.GetCustomAttribute<DynamoDBPropertyAttribute>();
        var collumnJsonName = property.GetCustomAttribute<JsonPropertyAttribute>();

        var collumn = collumnName?.AttributeName ?? 
                      collumnJsonName?.PropertyName ?? 
                      property.Name;
        return collumn;
    }
}
