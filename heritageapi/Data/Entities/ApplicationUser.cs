using Microsoft.AspNetCore.Identity;

namespace HeritageApi.Data.Entities;

public class ApplicationUser : IdentityUser<long>
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}

