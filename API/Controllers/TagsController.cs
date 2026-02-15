using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;
using API.DTOs;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TagsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tags = await _db.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();
        
        var tagDtos = tags.Select(t => new TagDto { Id = t.Id, Name = t.Name }).ToList();
        return Ok(tagDtos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid tag ID.");

        var tag = await _db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (tag is null)
            return NotFound();

        return Ok(new TagDto { Id = tag.Id, Name = tag.Name });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TagDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var validationError = await ValidateTagName(dto.Name, null);
        if (validationError != null)
            return BadRequest(validationError);

        var tag = new Tag { Name = dto.Name.Trim() };
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, 
            new TagDto { Id = tag.Id, Name = tag.Name });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TagDto dto)
    {
        if (id <= 0)
            return BadRequest("Invalid tag ID.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tag = await _db.Tags.FindAsync(id);
        if (tag is null)
            return NotFound();

        var validationError = await ValidateTagName(dto.Name, id);
        if (validationError != null)
            return BadRequest(validationError);

        tag.Name = dto.Name.Trim();
        await _db.SaveChangesAsync();

        return Ok(new TagDto { Id = tag.Id, Name = tag.Name });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid tag ID.");

        var tag = await _db.Tags
            .Include(t => t.TaskTags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag is null)
            return NotFound();

        // Check if tag is in use
        if (tag.TaskTags.Any())
            return BadRequest($"Cannot delete tag '{tag.Name}' because it is assigned to {tag.TaskTags.Count} task(s).");

        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidateTagName(string name, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Tag name cannot be empty or contain only whitespace.";

        var trimmedName = name.Trim();

        // Check for duplicate tag names (case-insensitive)
        var query = _db.Tags.Where(t => EF.Functions.Like(t.Name, trimmedName));
        
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);

        var existingTag = await query.FirstOrDefaultAsync();
        
        if (existingTag != null)
            return $"A tag with the name '{existingTag.Name}' already exists.";

        return null;
    }
}