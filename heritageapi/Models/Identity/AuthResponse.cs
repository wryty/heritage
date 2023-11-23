namespace HeritageApi.Models.Identity;

public class AuthResponse
{
    public string UserName { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}