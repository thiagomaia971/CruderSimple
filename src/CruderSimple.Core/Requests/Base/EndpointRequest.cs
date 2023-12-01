namespace CruderSimple.Core.Requests.Base;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class EndpointRequest(EndpointMethod method,
    string version,
    string endpoint,
    bool requireAuthorization = true,
    string[] roles = null) : Attribute
{
    public EndpointMethod EndpointMethod { get; } = method;
    public string Version { get; } = version;
    public string Endpoint { get; } = endpoint;
    public bool RequireAuthorization { get; } = requireAuthorization;
    public string[] Roles { get; } = roles;
}
