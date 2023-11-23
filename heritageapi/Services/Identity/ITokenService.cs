using Microsoft.AspNetCore.Identity;
using HeritageApi.Data.Entities;

namespace HeritageApi.Services.Identity;


public interface ITokenService
{
    string CreateToken(ApplicationUser user, List<IdentityRole<long>> role);
}