using API.DTOs;
using API.Entities;

namespace API.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Telephone = user.Telephone
        };
    }

    public static User ToEntity(CreateUserDto dto)
    {
        return new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            Telephone = dto.Telephone.Trim()
        };
    }

    public static void UpdateEntity(User user, UpdateUserDto dto)
    {
        user.FullName = dto.FullName.Trim();
        user.Email = dto.Email.Trim().ToLowerInvariant();
        user.Telephone = dto.Telephone.Trim();
    }

    public static List<UserDto> ToDtoList(IEnumerable<User> users)
    {
        return users.Select(ToDto).ToList();
    }
}
