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
using Microsoft.EntityFrameworkCore;

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


    [Authorize(AuthenticationSchemes = "Bearer", Roles = RoleConsts.Administrator)]
    [HttpPost("/api/Exhibit")]
    public async Task<ActionResult> CreateExhibit([FromBody] ExhibitRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.GetUserAsync(User);

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

        return Ok(new { ExhibitId = Exhibit.Id, Message = "Exhibit created successfully" });
    }

    [Authorize(AuthenticationSchemes = "Bearer", Roles = RoleConsts.Administrator)]
    [HttpPut("/api/Exhibit/{id}")]
    public async Task<ActionResult> UpdateExhibit(long id, [FromBody] ExhibitRequest request)
    {
        var user = await _userManager.GetUserAsync(User);

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

    [Authorize(AuthenticationSchemes = "Bearer", Roles = RoleConsts.Administrator)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExhibit(long id)
    {
        var Exhibit = await _context.Exhibits.FindAsync(id);

        var user = await _userManager.GetUserAsync(User);

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
        var exhibit = _context.Exhibits.Find(id);
        return Ok(new { exhibit.Id, exhibit.Name, exhibit.Description, exhibit.ImageFileName });
    }

    [HttpGet]
    public async Task<IActionResult> GetExhibits()
    {
        var exhibits = await _context.Exhibits.Select(e => new { e.Id, e.Name, e.Description, e.ImageFileName }).ToListAsync();
        return Ok(exhibits);
    }


    [HttpPost("/api/Exhibit/UploadImage/{id}")]
    public async Task<ActionResult> UploadImage(long id, IFormFile file)
    {
        var exhibit = await _context.Exhibits.FindAsync(id);

        if (exhibit == null)
        {
            return NotFound("Exhibit not found");
        }

        var uploadPath = Path.Combine("..", "heritage", "wwwroot", "uploads");

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        exhibit.ImageFileName = uniqueFileName;
        _context.Exhibits.Update(exhibit);
        await _context.SaveChangesAsync();

        return Ok("Image uploaded successfully");
    }



}
