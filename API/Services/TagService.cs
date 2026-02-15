using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class TagService : BaseService, ITagService
{
    public TagService(AppDbContext db) : base(db)
    {
    }

    public async Task<ServiceResult<TagDto>> GetByIdAsync(int id)
    {
        var tag = await Db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return tag is null
            ? ServiceResult<TagDto>.NotFound()
            : ServiceResult<TagDto>.Ok(new TagDto { Id = tag.Id, Name = tag.Name });
    }

    public async Task<ServiceResult<List<TagDto>>> GetAllAsync()
    {
        return await ExecuteQueryAsync(async () =>
        {
            var tags = await Db.Tags
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();

            return tags.Select(t => new TagDto { Id = t.Id, Name = t.Name }).ToList();
        }, "Failed to retrieve tags");
    }

    public async Task<ServiceResult<TagDto>> CreateAsync(TagDto dto)
    {
        var validationResult = ValidateOrFail<TagDto>(await ValidateTagName(dto.Name, null));
        if (validationResult != null)
            return validationResult;

        var tag = new Tag { Name = dto.Name.Trim() };

        return await ExecuteDatabaseOperationAsync(async () =>
        {
            Db.Tags.Add(tag);
            await Db.SaveChangesAsync();
            return new TagDto { Id = tag.Id, Name = tag.Name };
        }, "Failed to create tag");
    }

    public async Task<ServiceResult<TagDto>> UpdateAsync(int id, TagDto dto)
    {
        var tag = await Db.Tags.FindAsync(id);
        if (tag is null)
            return ServiceResult<TagDto>.NotFound();

        var validationResult = ValidateOrFail<TagDto>(await ValidateTagName(dto.Name, id));
        if (validationResult != null)
            return validationResult;

        tag.Name = dto.Name.Trim();

        return await ExecuteDatabaseOperationAsync(async () =>
        {
            await Db.SaveChangesAsync();
            return new TagDto { Id = tag.Id, Name = tag.Name };
        }, "Failed to update tag");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var tag = await Db.Tags
            .Include(t => t.TaskTags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag is null)
            return ServiceResult<bool>.NotFound();

        var dependencyCheck = ValidateNoDependencies<bool>(
            tag.TaskTags.Any(),
            $"tag '{tag.Name}'",
            "task(s)",
            tag.TaskTags.Count);
        if (dependencyCheck != null)
            return dependencyCheck;

        return await ExecuteDatabaseOperationAsync(async () =>
        {
            Db.Tags.Remove(tag);
            await Db.SaveChangesAsync();
            return true;
        }, "Failed to delete tag");
    }

    private async Task<string?> ValidateTagName(string name, int? excludeId)
    {
        var emptyCheck = ValidateNotEmpty(name, "Tag name");
        if (emptyCheck != null)
            return emptyCheck;

        var trimmedName = name.Trim();

        var query = Db.Tags.Where(t => t.Name.ToLower() == trimmedName.ToLower());
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);

        var existingTag = await query.FirstOrDefaultAsync();
        if (existingTag != null)
            return $"A tag with the name '{existingTag.Name}' already exists.";

        return null;
    }
}
