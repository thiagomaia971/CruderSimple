namespace CruderSimple.Core.ViewModels.Login;

public class LoginResult
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
    public LoginUserResult User { get; set; }
    public IEnumerable<LoginRouteResult> Routes { get; set; }
}
