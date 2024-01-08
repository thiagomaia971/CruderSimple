namespace CruderSimple.Core.ViewModels.Login;

public class LoginResult
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
}
