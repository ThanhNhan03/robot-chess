using robot_chess_api.DTOs;
using robot_chess_api.Models;
using robot_chess_api.Repositories;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Services.Implement;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(IFeedbackRepository feedbackRepository, ILogger<FeedbackService> logger)
    {
        _feedbackRepository = feedbackRepository;
        _logger = logger;
    }

    public async Task<List<FeedbackDto>> GetAllFeedbacksAsync()
    {
        var feedbacks = await _feedbackRepository.GetAllFeedbacksAsync();
        return feedbacks.Select(MapToDto).ToList();
    }

    public async Task<FeedbackDto?> GetFeedbackByIdAsync(Guid id)
    {
        var feedback = await _feedbackRepository.GetFeedbackByIdAsync(id);
        return feedback != null ? MapToDto(feedback) : null;
    }

    public async Task<List<FeedbackDto>> GetFeedbacksByUserIdAsync(Guid userId)
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksByUserIdAsync(userId);
        return feedbacks.Select(MapToDto).ToList();
    }

    public async Task<FeedbackDto> CreateFeedbackAsync(Guid userId, CreateFeedbackDto dto)
    {
        var feedback = new Feedback
        {
            UserId = userId,
            Message = dto.Message
        };

        var created = await _feedbackRepository.CreateFeedbackAsync(feedback);
        return MapToDto(created);
    }

    public async Task<FeedbackDto> UpdateFeedbackAsync(Guid id, Guid userId, UpdateFeedbackDto dto)
    {
        var feedback = await _feedbackRepository.GetFeedbackByIdAsync(id);
        
        if (feedback == null)
        {
            throw new KeyNotFoundException($"Feedback with ID {id} not found");
        }

        // Only the owner can update their feedback
        if (feedback.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own feedback");
        }

        if (!string.IsNullOrEmpty(dto.Message))
        {
            feedback.Message = dto.Message;
        }

        var updated = await _feedbackRepository.UpdateFeedbackAsync(feedback);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteFeedbackAsync(Guid id, Guid userId, bool isAdmin)
    {
        var feedback = await _feedbackRepository.GetFeedbackByIdAsync(id);
        
        if (feedback == null)
        {
            return false;
        }

        // Only admin or the owner can delete
        if (!isAdmin && feedback.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own feedback");
        }

        return await _feedbackRepository.DeleteFeedbackAsync(id);
    }

    private static FeedbackDto MapToDto(Feedback feedback)
    {
        return new FeedbackDto
        {
            Id = feedback.Id,
            UserId = feedback.UserId,
            UserEmail = feedback.User?.Email,
            UserFullName = feedback.User?.FullName,
            Message = feedback.Message,
            CreatedAt = feedback.CreatedAt
        };
    }
}
