using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class TagService : ITagService
{
    private readonly AppDbContext _db;

    public TagService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<TagDto>> GetByIdAsync(int id)
    {
        var tag = await _db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return tag is null
            ? ServiceResult<TagDto>.NotFound()
            : ServiceResult<TagDto>.Ok(new TagDto { Id = tag.Id, Name = tag.Name });
    }

    public async Task<ServiceResult<List<TagDto>>> GetAllAsync()
    {
        try
        {
            var tags = await _db.Tags
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();

            var tagDtos = tags.Select(t => new TagDto { Id = t.Id, Name = t.Name }).ToList();
            return ServiceResult<List<TagDto>>.Ok(tagDtos);
        }
        catch (Exception)
        {
            return ServiceResult<List<TagDto>>.Fail(ServiceErrorType.Database, "Failed to retrieve tags");
        }
    }

    public async Task<ServiceResult<TagDto>> CreateAsync(TagDto dto)
    {
        var validationError = await ValidateTagName(dto.Name, null);
        if (validationError != null)
            return ServiceResult<TagDto>.Fail(ServiceErrorType.Validation, validationError);

        var tag = new Tag { Name = dto.Name.Trim() };

        try
        {
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();
            return ServiceResult<TagDto>.Ok(new TagDto { Id = tag.Id, Name = tag.Name });
        }
        catch (DbUpdateException)
        {
            return ServiceResult<TagDto>.Fail(ServiceErrorType.Database, "Failed to create tag due to database error");
        }
    }

    public async Task<ServiceResult<TagDto>> UpdateAsync(int id, TagDto dto)
    {
        var tag = await _db.Tags.FindAsync(id);
        if (tag is null)
            return ServiceResult<TagDto>.NotFound();

        var validationError = await ValidateTagName(dto.Name, id);
        if (validationError != null)
            return ServiceResult<TagDto>.Fail(ServiceErrorType.Validation, validationError);

        tag.Name = dto.Name.Trim();

        try
        {
            await _db.SaveChangesAsync();
            return ServiceResult<TagDto>.Ok(new TagDto { Id = tag.Id, Name = tag.Name });
        }
        catch (DbUpdateException)
        {
            return ServiceResult<TagDto>.Fail(ServiceErrorType.Database, "Failed to update tag due to database error");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var tag = await _db.Tags
            .Include(t => t.TaskTags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag is null)
            return ServiceResult<bool>.NotFound();

        if (tag.TaskTags.Any())
        {
            return ServiceResult<bool>.Fail(
                ServiceErrorType.Validation,
                $"Cannot delete tag '{tag.Name}' because it is assigned to {tag.TaskTags.Count} task(s)."
            );
        }

        try
        {
            _db.Tags.Remove(tag);
            await _db.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }
        catch (DbUpdateException)
        {
            return ServiceResult<bool>.Fail(ServiceErrorType.Database, "Failed to delete tag due to database error");
        }
    }

    private async Task<string?> ValidateTagName(string name, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Tag name cannot be empty or contain only whitespace.";

        var trimmedName = name.Trim();

        var query = _db.Tags.Where(t => t.Name.ToLower() == trimmedName.ToLower());
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);

        var existingTag = await query.FirstOrDefaultAsync();
        if (existingTag != null)
            return $"A tag with the name '{existingTag.Name}' already exists.";

        return null;
    }
}
