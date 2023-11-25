namespace CruderSimple.Core.Requests;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class EndpointRequest(
    EndpointMethod endpointMethod, 
    string version, 
    string endpoint, 
    bool isMultiTenant = true,
    string[]? authorizeRole = null) : Attribute
{
    public EndpointMethod EndpointMethod { get; } = endpointMethod;
    public string Version { get; } = version;
    public string Endpoint { get; } = endpoint;
    public bool IsMultiTenant { get; } = isMultiTenant;
    public string[]? AuthorizeRole { get; } = authorizeRole;
}