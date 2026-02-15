using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class UserService : BaseService, IUserService
{
    public UserService(AppDbContext db) : base(db)
    {
    }

    public async Task<ServiceResult<UserDto>> GetByIdAsync(int id)
    {
        var user = await Db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return user is null
            ? ServiceResult<UserDto>.NotFound()
            : ServiceResult<UserDto>.Ok(MapToDto(user));
    }

    public async Task<ServiceResult<List<UserDto>>> GetAllAsync()
    {
        return await ExecuteQueryAsync(async () =>
        {
            var users = await Db.Users
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return users.Select(MapToDto).ToList();
        }, "Failed to retrieve users");
    }

    public async Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto)
    {
        var validationResult = ValidateOrFail<UserDto>(await ValidateUser(dto.Email, dto.Telephone, null));
        if (validationResult != null)
            return validationResult;

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            Telephone = dto.Telephone.Trim()
        };

        return await ExecuteDatabaseOperationAsync(async () =>
        {
            Db.Users.Add(user);
            await Db.SaveChangesAsync();
            return MapToDto(user);
        }, "Failed to create user");
    }

    public async Task<ServiceResult<UserDto>> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await Db.Users.FindAsync(id);
        if (user is null)
            return ServiceResult<UserDto>.NotFound();

        var validationResult = ValidateOrFail<UserDto>(await ValidateUser(dto.Email, dto.Telephone, id));
        if (validationResult != null)
            return validationResult;

        user.FullName = dto.FullName.Trim();
        user.Email = dto.Email.Trim().ToLowerInvariant();
        user.Telephone = dto.Telephone.Trim();

        return await ExecuteDatabaseOperationAsync(async () =>
        {
            await Db.SaveChangesAsync();
            return MapToDto(user);
        }, "Failed to update user");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var user = await Db.Users
            .Include(u => u.Tasks)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return ServiceResult<bool>.NotFound();

        var dependencyCheck = ValidateNoDependencies<bool>(
            user.Tasks.Any(),
            $"user '{user.FullName}'",
            "assigned task(s)",
            user.Tasks.Count);
        if (dependencyCheck != null)
            return dependencyCheck;

        return await ExecuteDatabaseOperationAsync(async () =>
        {
            Db.Users.Remove(user);
            await Db.SaveChangesAsync();
            return true;
        }, "Failed to delete user");
    }

    private async Task<string?> ValidateUser(string email, string telephone, int? excludeId)
    {
        var emailCheck = ValidateNotEmpty(email, "Email");
        if (emailCheck != null)
            return emailCheck;

        var telephoneCheck = ValidateNotEmpty(telephone, "Telephone");
        if (telephoneCheck != null)
            return telephoneCheck;

        var trimmedEmail = email.Trim().ToLowerInvariant();
        var trimmedTelephone = telephone.Trim();

        var emailQuery = Db.Users.Where(u => u.Email == trimmedEmail);
        if (excludeId.HasValue)
            emailQuery = emailQuery.Where(u => u.Id != excludeId.Value);

        var existingUserByEmail = await emailQuery.FirstOrDefaultAsync();
        if (existingUserByEmail != null)
            return $"A user with the email '{existingUserByEmail.Email}' already exists.";

        var telephoneQuery = Db.Users.Where(u => u.Telephone == trimmedTelephone);
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
