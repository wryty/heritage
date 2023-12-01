using System.Net.Http;
using System.Threading.Tasks;


namespace Heritage.Services;
using Microsoft.AspNetCore.Components.Authorization;

public class AuthenticationServices
{
    private readonly HttpClient httpClient;
    private readonly AuthenticationStateProvider authenticationStateProvider;
    private readonly SessionService sessionService;

    public AuthenticationServices(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, SessionService sessionService)
    {
        this.httpClient = httpClient;
        this.authenticationStateProvider = authenticationStateProvider;
        this.sessionService = sessionService;
    }

    public async Task<string?> AuthenticateAsync(string username, string password)
    {
        var response = await httpClient.PostAsJsonAsync("https://localhost:7210/api/Account/SignIn", new { Username = username, Password = password });

        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

            if (tokenResponse != null)
            {
                await sessionService.SetTokenAsync(tokenResponse.Token);
                return tokenResponse.Token;
            }
        }

        return null;
    }

    private class TokenResponse
    {
        public string Token { get; set; }
    }
}
