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

namespace HeritageApi.Controllers;




[ApiController]
[Route("/api/Admin/Account")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = RoleConsts.Administrator)]
public class AdminAccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AdminAccountController(ITokenService tokenService, DataContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _tokenService = tokenService;
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }


    [HttpGet]
    public IActionResult GetUsers()
    {
        return Ok(new { _userManager.Users });
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return Ok(new { user });
    }

    [HttpPost]
    public async Task<ActionResult<AuthResponse>> CreateAccountAdmin([FromBody] AdminRegisterRequest request)
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


    [HttpPut("{id}")]
    public async Task<ActionResult<AuthResponse>> UpdateAccountAdmin([FromBody] AdminRegisterRequest request, string id)
    {
        var user = await _userManager.FindByIdAsync(id);

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
        await _userManager.RemoveFromRolesAsync(user, _userManager.GetRolesAsync(user).Result);
        await _userManager.AddToRoleAsync(user, request.IsAdmin ? RoleConsts.Administrator : RoleConsts.Member);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccountAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            return Ok("User deleted successfully");
        }
        else
        {
            return BadRequest("Failed to delete user");
        }
    }
}
