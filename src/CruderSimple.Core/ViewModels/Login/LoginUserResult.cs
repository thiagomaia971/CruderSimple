namespace CruderSimple.Core.ViewModels.Login;

public class LoginUserResult
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string CompanyId { get; set; }
    public string Name { get; set; }
    public IEnumerable<string> Roles { get; set; }
    public IEnumerable<string> Permissions { get; set; }
}
