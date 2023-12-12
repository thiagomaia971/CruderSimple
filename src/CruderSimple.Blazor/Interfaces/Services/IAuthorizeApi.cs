
using CruderSimple.Blazor.Services;
using CruderSimple.Core.ViewModels.Login;

namespace CruderSimple.Blazor.Interfaces.Services;

public interface IAuthorizeApi
{
    Task<LoginResult> Login(LoginViewModel login);
    //Task Register(UserInput userInput);
}
