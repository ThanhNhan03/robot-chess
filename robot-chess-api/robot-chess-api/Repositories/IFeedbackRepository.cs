using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

public interface IFeedbackRepository
{
    Task<List<Feedback>> GetAllFeedbacksAsync();
    Task<Feedback?> GetFeedbackByIdAsync(Guid id);
    Task<List<Feedback>> GetFeedbacksByUserIdAsync(Guid userId);
    Task<Feedback> CreateFeedbackAsync(Feedback feedback);
    Task<Feedback> UpdateFeedbackAsync(Feedback feedback);
    Task<bool> DeleteFeedbackAsync(Guid id);
}
