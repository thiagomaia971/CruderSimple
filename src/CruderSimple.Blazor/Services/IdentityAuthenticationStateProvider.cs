using System.Security.Claims;
using Blazored.LocalStorage;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.ViewModels.Login;
using Microsoft.AspNetCore.Components.Authorization;

namespace CruderSimple.Blazor.Services;

public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IAuthorizeApi _authorizeApi;
    private readonly ILocalStorageService localStorage;
    public static LoginResult UserCache;

    public IdentityAuthenticationStateProvider(IAuthorizeApi authorizeApi, ILocalStorageService localStorage)
    {
        this._authorizeApi = authorizeApi;
        this.localStorage = localStorage;
    }

    public async Task Login(LoginViewModel login)
    {
        var loginResult = await _authorizeApi.Login(login);
        await localStorage.SetItemAsync("identity", loginResult);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    //public async Task Register(UserInput userInput)
    //{
    //    await _authorizeApi.Register(userInput);
    //    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    //}

    public async Task Logout()
    {
        await localStorage.RemoveItemAsync("identity");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<LoginResult> GetUserInfo()
    {
        LoginResult loginResult = await localStorage.GetItemAsync<LoginResult>("identity");
        return loginResult;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var login = await GetUserInfo();
        UserCache = login;

        var identity = new ClaimsIdentity();

        if (login is not null)
        {
            var authClaims = new List<Claim>
            {
                new Claim("UserId", login.User.Id),
                new Claim("TenantId", login.User.CompanyId),
                new Claim(ClaimTypes.Name, login.User.Name),
                new Claim(ClaimTypes.Role, string.Join(",", login.User.Roles)),
                new Claim("Permissions", string.Join(",", login.User.Permissions))
            };
            identity = new ClaimsIdentity(authClaims, "Server authentication");
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}
