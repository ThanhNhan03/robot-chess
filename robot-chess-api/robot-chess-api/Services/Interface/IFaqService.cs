using robot_chess_api.DTOs;

namespace robot_chess_api.Services.Interface;

public interface IFaqService
{
    Task<List<FaqDto>> GetAllFaqsAsync(bool includeUnpublished = false);
    Task<FaqDto?> GetFaqByIdAsync(Guid id);
    Task<List<FaqDto>> GetFaqsByCategoryAsync(string category);
    Task<FaqDto> CreateFaqAsync(CreateFaqDto dto);
    Task<FaqDto> UpdateFaqAsync(Guid id, UpdateFaqDto dto);
    Task<bool> DeleteFaqAsync(Guid id);
    Task<List<string>> GetCategoriesAsync();
}
