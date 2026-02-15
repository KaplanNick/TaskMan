using API.DTOs;
using API.Services;

namespace API.Interfaces;

public interface ITaskService
{
    Task<ServiceResult<TaskDto>> CreateAsync(CreateTaskDto dto);
    Task<ServiceResult<TaskDto>> GetByIdAsync(int id);
    Task<ServiceResult<List<TaskDto>>> GetAllAsync();
    Task<ServiceResult<TaskDto>> UpdateAsync(int id, UpdateTaskDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
