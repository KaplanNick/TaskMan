using API.DTOs;
using API.Services;

namespace API.Interfaces;

public interface IUserService
{
    Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto);
    Task<ServiceResult<UserDto>> GetByIdAsync(int id);
    Task<ServiceResult<List<UserDto>>> GetAllAsync();
    Task<ServiceResult<UserDto>> UpdateAsync(int id, UpdateUserDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
