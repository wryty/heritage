namespace Heritage.Services;

using Heritage.Entites;
using HeritageApi.Data.Entities;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


public class SessionService
{
    private readonly ProtectedSessionStorage protectedSessionStorage;
    public event Action RolesChanged;

    public SessionService(ProtectedSessionStorage protectedSessionStorage)
    {
        this.protectedSessionStorage = protectedSessionStorage;
        LoadUserFromStorage();
    }

    public MuseumUser CurrentUser { get; private set; } = new MuseumUser();

    private async Task LoadUserFromStorage()
    {
        var userResult = await protectedSessionStorage.GetAsync<MuseumUser>("CurrentUser");
        if (userResult.Success)
        {
            CurrentUser = userResult.Value;
            RolesChanged?.Invoke();
        }
    }

    public async Task SetTokenAsync(string token)
    {
        await protectedSessionStorage.SetAsync("AccessToken", token);
        var user = ExtractUserDataFromToken(token);
        await SetUserAsync(user);
    }

    public async Task<string?> GetTokenAsync()
    {
        var result = await protectedSessionStorage.GetAsync<string>("AccessToken");
        return result.Success ? result.Value : null;
    }

    public async Task ClearTokenAsync()
    {
        await protectedSessionStorage.DeleteAsync("AccessToken");
        CurrentUser = new MuseumUser();
        RolesChanged?.Invoke();
    }

    private MuseumUser ExtractUserDataFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

        var roles = jsonToken?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)   
            .ToList();

        var name = jsonToken?.Claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault().Value;
        var userId = jsonToken?.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;

        return new MuseumUser
        {
            UserId = userId,
            UserName = name,
            Roles = roles ?? new List<string>()
        };
    }

    public async Task SetUserAsync(MuseumUser user)
    {
        CurrentUser = user;
        await protectedSessionStorage.SetAsync("CurrentUser", user);
        RolesChanged?.Invoke();
    }

    public async Task<MuseumUser?> GetUserAsync()
    {
        var result = await protectedSessionStorage.GetAsync<MuseumUser>("CurrentUser");
        return result.Success ? result.Value : null;
    }
}