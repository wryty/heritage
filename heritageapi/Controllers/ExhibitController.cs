using HeritageApi.Data.Entities;
using HeritageApi.Data;
using HeritageApi.Models.Identity;
using HeritageApi.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.Xml;
using HeritageApi.Models;

namespace HeritageApi.Controllers;


[ApiController]
[Route("/api/Exhibit")]
public class ExhibitController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public ExhibitController(ITokenService tokenService, DataContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _tokenService = tokenService;
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }


    [Authorize]
    [HttpPost("/api/Exhibit")]
    public async Task<ActionResult> CreateExhibit([FromBody] ExhibitRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByNameAsync(User.Identity.Name);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var Exhibit = new Exhibit
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Exhibits.Add(Exhibit);
        await _context.SaveChangesAsync();

        return Ok("Exhibit created successfully");
    }

    [Authorize]
    [HttpPut("/api/Exhibit/{id}")]
    public async Task<ActionResult> UpdateExhibit(long id, [FromBody] ExhibitRequest request)
    {
        var user = await _userManager.FindByNameAsync(User.Identity.Name);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var exhibit = await _context.Exhibits.FindAsync(id);

        if (exhibit == null)
        {
            return NotFound("Exhibit not found");
        }

        exhibit.Name = request.Name;
        exhibit.Description = request.Description;  

        _context.Exhibits.Update(exhibit);
        await _context.SaveChangesAsync();
        
        return Ok("Exhibit updated successfully");
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExhibit(long id)
    {
        var Exhibit = await _context.Exhibits.FindAsync(id);

        var user = await _userManager.FindByNameAsync(User.Identity.Name);

        if (Exhibit == null)
        {
            return NotFound("Exhibit not found");
        }
        _context.Exhibits.Remove(Exhibit);

        var result = await _context.SaveChangesAsync();

        if (result > 0)
        {
            return Ok("Exhibit deleted successfully");
        }
        else
        {
            return BadRequest("Failed to delete Exhibit");
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetExhibitByIdAdmin(long id)
    {
        return Ok(_context.Exhibits.Find(id));
    }


    [HttpGet]
    public async Task<IActionResult> GetExhibits()
    {
        return Ok( _context.Exhibits );
    }

}
