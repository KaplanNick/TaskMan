using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<UserDto>> GetByIdAsync(int id)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return user is null
            ? ServiceResult<UserDto>.NotFound()
            : ServiceResult<UserDto>.Ok(MapToDto(user));
    }

    public async Task<ServiceResult<List<UserDto>>> GetAllAsync()
    {
        try
        {
            var users = await _db.Users
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var userDtos = users.Select(MapToDto).ToList();
            return ServiceResult<List<UserDto>>.Ok(userDtos);
        }
        catch (Exception)
        {
            return ServiceResult<List<UserDto>>.Fail(ServiceErrorType.Database, "Failed to retrieve users");
        }
    }

    public async Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto)
    {
        var validationError = await ValidateUser(dto.Email, dto.Telephone, null);
        if (validationError != null)
            return ServiceResult<UserDto>.Fail(ServiceErrorType.Validation, validationError);

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
            return ServiceResult<UserDto>.Ok(MapToDto(user));
        }
        catch (DbUpdateException)
        {
            return ServiceResult<UserDto>.Fail(ServiceErrorType.Database, "Failed to create user due to database error");
        }
    }

    public async Task<ServiceResult<UserDto>> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null)
            return ServiceResult<UserDto>.NotFound();

        var validationError = await ValidateUser(dto.Email, dto.Telephone, id);
        if (validationError != null)
            return ServiceResult<UserDto>.Fail(ServiceErrorType.Validation, validationError);

        user.FullName = dto.FullName.Trim();
        user.Email = dto.Email.Trim().ToLowerInvariant();
        user.Telephone = dto.Telephone.Trim();

        try
        {
            await _db.SaveChangesAsync();
            return ServiceResult<UserDto>.Ok(MapToDto(user));
        }
        catch (DbUpdateException)
        {
            return ServiceResult<UserDto>.Fail(ServiceErrorType.Database, "Failed to update user due to database error");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var user = await _db.Users
            .Include(u => u.Tasks)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return ServiceResult<bool>.NotFound();

        if (user.Tasks.Any())
        {
            return ServiceResult<bool>.Fail(
                ServiceErrorType.Validation,
                $"Cannot delete user '{user.FullName}' because they have {user.Tasks.Count} assigned task(s)."
            );
        }

        try
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }
        catch (DbUpdateException)
        {
            return ServiceResult<bool>.Fail(ServiceErrorType.Database, "Failed to delete user due to database error");
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

        var emailQuery = _db.Users.Where(u => u.Email == trimmedEmail);
        if (excludeId.HasValue)
            emailQuery = emailQuery.Where(u => u.Id != excludeId.Value);

        var existingUserByEmail = await emailQuery.FirstOrDefaultAsync();
        if (existingUserByEmail != null)
            return $"A user with the email '{existingUserByEmail.Email}' already exists.";

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
