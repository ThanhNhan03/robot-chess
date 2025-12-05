using robot_chess_api.DTOs;
using System;
using System.Threading.Tasks;

namespace robot_chess_api.Services.Interface
{
    public interface IAiSuggestionService
    {
        /// <summary>
        /// Get AI chess move suggestion for current position
        /// Only deducts points from user - does NOT save to ai_suggestions table
        /// </summary>
        /// <param name="userId">User requesting the suggestion</param>
        /// <param name="request">Suggestion request with FEN position</param>
        /// <returns>Suggestion response with best move and remaining points</returns>
        Task<SuggestionResponseDto> GetSuggestionAsync(Guid userId, GetSuggestionRequestDto request);

        /// <summary>
        /// Get suggestion cost in points
        /// </summary>
        /// <returns>Points required per suggestion</returns>
        int GetSuggestionCost();

        /// <summary>
        /// Check rate limit for user (prevent spam)
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user can make request, false if rate limited</returns>
        Task<bool> CheckRateLimitAsync(Guid userId);
    }
}
