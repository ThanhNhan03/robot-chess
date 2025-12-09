using robot_chess_api.DTOs;

namespace robot_chess_api.Services.Interface;

public interface IFeedbackService
{
    Task<List<FeedbackDto>> GetAllFeedbacksAsync();
    Task<FeedbackDto?> GetFeedbackByIdAsync(Guid id);
    Task<List<FeedbackDto>> GetFeedbacksByUserIdAsync(Guid userId);
    Task<FeedbackDto> CreateFeedbackAsync(Guid userId, CreateFeedbackDto dto);
    Task<FeedbackDto> UpdateFeedbackAsync(Guid id, Guid userId, UpdateFeedbackDto dto);
    Task<bool> DeleteFeedbackAsync(Guid id, Guid userId, bool isAdmin);
}
