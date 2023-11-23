using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HeritageApi.Data.Entities;
using HeritageApi.Data;
using System.IdentityModel.Tokens.Jwt;
using HeritageApi.Services.Identity;
using HeritageApi.Models.Identity;
using Microsoft.EntityFrameworkCore;
using HeritageApi.Extensions;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Web;
using HeritageApi.Data.Entities;
using HeritageApi.Data;

namespace HeritageApi.Controllers;



[ApiController]
[Route("/api/Account")]
public class AccountsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AccountsController(ITokenService tokenService, DataContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _tokenService = tokenService;
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("SignIn")]
    public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var managedUser = await _userManager.FindByNameAsync(request.UserName);

        if (managedUser == null)
        {
            return BadRequest("Bad credentials");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password);

        if (!isPasswordValid)
        {
            return BadRequest("Bad credentials");
        }

        var user = _context.Users.FirstOrDefault(u => u.UserName == request.UserName);

        if (user is null)
            return Unauthorized();

        var roleIds = await _context.UserRoles.Where(r => r.UserId == user.Id).Select(x => x.RoleId).ToListAsync();
        var roles = _context.Roles.Where(x => roleIds.Contains(x.Id)).ToList();

        var accessToken = _tokenService.CreateToken(user, roles);
        user.RefreshToken = _configuration.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_configuration.GetSection("Jwt:RefreshTokenValidityInDays").Get<int>());

        await _context.SaveChangesAsync();
        return Ok(new AuthResponse
        {
            UserName = user.UserName!,
            Token = accessToken,
            RefreshToken = user.RefreshToken
        });
    }

    [HttpPost("SignUp")]
    public async Task<ActionResult<AuthResponse>> SignUp([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(request);

        var user = new ApplicationUser
        {
            UserName = request.UserName
        };
        var result = await _userManager.CreateAsync(user, request.Password);

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        if (!result.Succeeded) return BadRequest(request);

        var findUser = await _context.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName);

        if (findUser == null) throw new Exception($"User {request.UserName} not found");

        await _userManager.AddToRoleAsync(findUser, RoleConsts.Member);

        return await Authenticate(new AuthRequest
        {
            UserName = request.UserName,
            Password = request.Password
        });
    }

    [HttpPost]
    [Route("refresh-token")]
    public async Task<IActionResult> RefreshToken(TokenModel? tokenModel)
    {
        if (tokenModel is null)
        {
            return BadRequest("Invalid client request");
        }

        var accessToken = tokenModel.AccessToken;
        var refreshToken = tokenModel.RefreshToken;
        var principal = _configuration.GetPrincipalFromExpiredToken(accessToken);

        if (principal == null)
        {
            return BadRequest("Invalid access token or refresh token");
        }

        var username = principal.Identity!.Name;
        var user = await _userManager.FindByNameAsync(username!);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return BadRequest("Invalid access token or refresh token");
        }

        var newAccessToken = _configuration.CreateToken(principal.Claims.ToList());
        var newRefreshToken = _configuration.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await _userManager.UpdateAsync(user);

        return new ObjectResult(new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
            refreshToken = newRefreshToken
        });
    }

    [Authorize]
    [HttpPost]
    [Route("revoke/{username}")]
    public async Task<IActionResult> Revoke(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return BadRequest("Invalid user name");

        user.RefreshToken = null;
        await _userManager.UpdateAsync(user);

        return Ok();
    }

    [Authorize]
    [HttpPost]
    [Route("revoke-all")]
    public async Task<IActionResult> RevokeAll()
    {
        var users = _userManager.Users.ToList();
        foreach (var user in users)
        {
            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
        }

        return Ok();
    }



    [Authorize]
    [HttpGet("Me")] // need to refac
    public async Task<IActionResult> GetMe()
    {
        var user = await _userManager.GetUserAsync(User);

        return Ok(new { user });
    }


    [Authorize]
    [HttpPost("Update")]
    public async Task<IActionResult> UpdateAccount([FromBody] UpdateRequest request)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound("User not found");
        }

        user.UserName = request.UserName;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return BadRequest("Failed to update username");
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, request.Password);

        if (resetResult.Succeeded)
        {
            await _context.SaveChangesAsync();
            return Ok("User information updated successfully");
        }
        else
        {
            return BadRequest("Failed to reset password");
        }
    }

    [Authorize]
    [HttpPost("SignOut")]
    public async Task<IActionResult> SignOut()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound("User not found");
        }

        user.RefreshToken = null;
        await _userManager.UpdateAsync(user);

        return Ok("User signed out successfully");
    }
}
