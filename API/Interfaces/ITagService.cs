using API.DTOs;
using API.Services;

namespace API.Interfaces;

public interface ITagService
{
    Task<ServiceResult<TagDto>> CreateAsync(TagDto dto);
    Task<ServiceResult<TagDto>> GetByIdAsync(int id);
    Task<ServiceResult<List<TagDto>>> GetAllAsync();
    Task<ServiceResult<TagDto>> UpdateAsync(int id, TagDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
