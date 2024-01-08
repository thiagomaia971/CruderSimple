using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.ViewModels.Login;
using Microsoft.AspNetCore.Components.Authorization;

namespace CruderSimple.Blazor.Services;

public class IdentityAuthenticationStateProvider : AuthenticationStateProvider, IIdentityAuthenticationStateProvider
{
    private readonly IAuthorizeApi _authorizeApi;
    private readonly ILocalStorageService localStorage;

    public IdentityAuthenticationStateProvider(IAuthorizeApi authorizeApi, ILocalStorageService localStorage)
    {
        this._authorizeApi = authorizeApi;
        this.localStorage = localStorage;
    }

    public async Task<LoginResult> Login(LoginViewModel login)
    {
        var loginResult = await _authorizeApi.Login(login);
        Console.WriteLine($"Saving: {JsonSerializer.Serialize(loginResult)}");
        await localStorage.SetItemAsync("identity", loginResult);
        // NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return loginResult;
    }

    //public async Task Register(UserInput userInput)
    //{
    //    await _authorizeApi.Register(userInput);
    //    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    //}

    public async Task Logout()
    {
        await localStorage.RemoveItemAsync("identity");
        await localStorage.RemoveItemAsync("claims");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public LoginResult UserInfoCached { get; set; }

    public async Task<LoginResult> GetUserInfo()
    {
        LoginResult loginResult = await localStorage.GetItemAsync<LoginResult>("identity");
        return loginResult;
    }
    
    public async Task ChangeClaims(params (string key, string value)[] claims)
    {
        await localStorage.RemoveItemAsync("claims");
        await localStorage.SetItemAsync("claims", claims.ToDictionary(c => c.key, c => c.value));
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var login = await GetUserInfo();
        var claims = await localStorage.GetItemAsync<Dictionary<string, string>>("claims");
        UserInfoCached = login;

        var identity = new ClaimsIdentity();
        var claimsIdentity = new List<Claim>();
        if (login is not null && claims is not null)
        {
            if (!claims.ContainsKey("UserId"))
                claimsIdentity.Add(new Claim("UserId", login.UserId));
            
            if (!claims.ContainsKey(ClaimTypes.Name))
                claimsIdentity.Add(new Claim(ClaimTypes.Name, login.UserName));
            
            claimsIdentity.AddRange(claims.Select(c => new Claim(c.Key, c.Value)));
            identity = new ClaimsIdentity(claimsIdentity, "Server authentication");
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}