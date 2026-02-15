using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;
using API.DTOs;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _db.Users
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .ToListAsync();
            
            var userDtos = users.Select(MapToDto).ToList();
            return Ok(userDtos);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Failed to retrieve users" });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid user ID." });

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return NotFound();

        return Ok(MapToDto(user));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { message = errors.FirstOrDefault() ?? "Invalid input" });
        }

        var validationError = await ValidateUser(dto.Email, dto.Telephone, null);
        if (validationError != null)
            return BadRequest(new { message = validationError });

        var user = new User 
        { 
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            Telephone = dto.Telephone.Trim()
        };

        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, MapToDto(user));
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, new { message = "Failed to create user due to database error" });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid user ID." });

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { message = errors.FirstOrDefault() ?? "Invalid input" });
        }

        var user = await _db.Users.FindAsync(id);
        if (user is null)
            return NotFound();

        var validationError = await ValidateUser(dto.Email, dto.Telephone, id);
        if (validationError != null)
            return BadRequest(new { message = validationError });

        user.FullName = dto.FullName.Trim();
        user.Email = dto.Email.Trim().ToLowerInvariant();
        user.Telephone = dto.Telephone.Trim();

        try
        {
            await _db.SaveChangesAsync();
            return Ok(MapToDto(user));
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, new { message = "Failed to update user due to database error" });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid user ID." });

        var user = await _db.Users
            .Include(u => u.Tasks)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound();

        // Check if user has tasks
        if (user.Tasks.Any())
            return BadRequest(new { message = $"Cannot delete user '{user.FullName}' because they have {user.Tasks.Count} assigned task(s)." });

        try
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, new { message = "Failed to delete user due to database error" });
        }
    }

    private async Task<string?> ValidateUser(string email, string telephone, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "Email cannot be empty or contain only whitespace.";

        if (string.IsNullOrWhiteSpace(telephone))
            return "Telephone cannot be empty or contain only whitespace.";

        var trimmedEmail = email.Trim().ToLowerInvariant();
        var trimmedTelephone = telephone.Trim();

        // Check for duplicate email (case-insensitive)
        // Email is already stored lowercase in DB, so direct comparison is safe
        var emailQuery = _db.Users.Where(u => u.Email == trimmedEmail);
        if (excludeId.HasValue)
            emailQuery = emailQuery.Where(u => u.Id != excludeId.Value);

        var existingUserByEmail = await emailQuery.FirstOrDefaultAsync();
        if (existingUserByEmail != null)
            return $"A user with the email '{existingUserByEmail.Email}' already exists.";

        // Check for duplicate telephone
        var telephoneQuery = _db.Users.Where(u => u.Telephone == trimmedTelephone);
        if (excludeId.HasValue)
            telephoneQuery = telephoneQuery.Where(u => u.Id != excludeId.Value);

        var existingUserByTelephone = await telephoneQuery.FirstOrDefaultAsync();
        if (existingUserByTelephone != null)
            return $"A user with the telephone number '{existingUserByTelephone.Telephone}' already exists.";

        return null;
    }

    private static UserDto MapToDto(User user) =>
        new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Telephone = user.Telephone
        };
}