using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

public interface IFaqRepository
{
    Task<List<Faq>> GetAllFaqsAsync(bool includeUnpublished = false);
    Task<Faq?> GetFaqByIdAsync(Guid id);
    Task<List<Faq>> GetFaqsByCategoryAsync(string category);
    Task<Faq> CreateFaqAsync(Faq faq);
    Task<Faq> UpdateFaqAsync(Faq faq);
    Task<bool> DeleteFaqAsync(Guid id);
    Task<List<string>> GetCategoriesAsync();
}
