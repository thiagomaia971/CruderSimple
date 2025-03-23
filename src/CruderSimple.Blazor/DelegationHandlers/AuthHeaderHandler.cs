using Microsoft.AspNetCore.Components.Authorization;

namespace CruderSimple.Blazor.DelegationHandlers;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthHeaderHandler(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();

        if (state.User.Identity.IsAuthenticated)
        {
            foreach (var claim in state.User.Claims)
            {
                string headerName = claim.Type.Split("/").LastOrDefault(); // Pegando a última parte do Type como Header
                if (!string.IsNullOrEmpty(headerName))
                {
                    if (headerName == "Token")
                        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {claim.Value}");
                    else
                        request.Headers.TryAddWithoutValidation(headerName, claim.Value);
                }
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}