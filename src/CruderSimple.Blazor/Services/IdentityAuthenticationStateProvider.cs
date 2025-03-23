using System.Security.Claims;
using System.Text;
using Blazored.LocalStorage;
using CruderSimple.Blazor.Interfaces.Services;
using CruderSimple.Core.ViewModels.Login;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace CruderSimple.Blazor.Services;

public class IdentityAuthenticationStateProvider : AuthenticationStateProvider, IIdentityAuthenticationStateProvider
{
    private readonly IAuthorizeApi _authorizeApi;
    private readonly ILocalStorageService localStorage;
    private byte xorConstant = 0x53;

    public IdentityAuthenticationStateProvider(IAuthorizeApi authorizeApi, ILocalStorageService localStorage)
    {
        this._authorizeApi = authorizeApi;
        this.localStorage = localStorage;
    }

    public async Task<LoginResult> Login(LoginViewModel login)
    {
        var loginResult = await _authorizeApi.Login(login);
        await SaveItem("identity", loginResult);
        return loginResult;
    }

    public async Task Logout()
    {
        await localStorage.RemoveItemAsync("identity");
        await localStorage.RemoveItemAsync("tenant");
        await localStorage.RemoveItemAsync("claims");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public LoginResult UserInfoCached { get; set; }

    public async Task<LoginResult> GetUserInfo()
    {
        var loginResult = await RetrivieItem<LoginResult>("identity");
        return loginResult;
    }
    
    public async Task ChangeClaims(params UserClaim[] claims)
    {
        await SaveItem("claims", claims.ToList());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var login = await GetUserInfo();
        var claims = await RetrivieItem<List<UserClaim>>("claims");
        UserInfoCached = login;

        var identity = new ClaimsIdentity();
        var claimsIdentity = new List<Claim>();
        if (login is not null && claims is not null)
        {
            if (!claims.Any(x => x.Key == "UserId"))
                claimsIdentity.Add(new Claim("UserId", login.UserId));
            
            if (!claims.Any(x => x.Key == ClaimTypes.Name))
                claimsIdentity.Add(new Claim(ClaimTypes.Name, login.UserName));
            claimsIdentity.Add(new Claim("Token", login.Token));
            
            claimsIdentity.AddRange(claims.Select(c => new Claim(c.Key, c.Value)));
            identity = new ClaimsIdentity(claimsIdentity, "Server authentication");
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private async Task SaveItem(string key, object value)
    {
        var encrypted = await EncryptAsync(JsonConvert.SerializeObject(value));
        await localStorage.SetItemAsync(key, encrypted);
    }

    private async Task<T> RetrivieItem<T>(string key)
        where T : class
    {
        var result = await localStorage.GetItemAsync<string>(key);
        if (result == null)
            return null;
        var encripted = await DecryptAsync(result);
        return JsonConvert.DeserializeObject<T>(encripted);
    }

    private async Task<string> EncryptAsync(string clearText)
    {
        byte[] data = Encoding.UTF8.GetBytes(clearText);
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(data[i] ^ xorConstant);
        }
        return Convert.ToBase64String(data);
    }

    public async Task<string> DecryptAsync(string encrypted)
    {
        byte[] data = Convert.FromBase64String(encrypted);
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(data[i] ^ xorConstant);
        }

        return Encoding.UTF8.GetString(data);
    }
}

public record UserClaim(string Key, string Value);