using CruderSimple.Core.ViewModels.Login;

namespace CruderSimple.Blazor.Interfaces.Services;

public interface IIdentityAuthenticationStateProvider
{
    LoginResult UserInfoCached { get; set; }
    Task<LoginResult> GetUserInfo();
}