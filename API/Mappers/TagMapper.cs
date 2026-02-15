using API.DTOs;
using API.Entities;

namespace API.Mappers;

public static class TagMapper
{
    public static TagDto ToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }

    public static Tag ToEntity(TagDto dto)
    {
        return new Tag
        {
            Id = dto.Id,
            Name = dto.Name.Trim()
        };
    }

    public static List<TagDto> ToDtoList(IEnumerable<Tag> tags)
    {
        return tags.Select(ToDto).ToList();
    }
}
