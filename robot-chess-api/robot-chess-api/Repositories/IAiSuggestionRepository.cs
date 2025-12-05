using robot_chess_api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace robot_chess_api.Repositories
{
    public interface IAiSuggestionRepository
    {
        /// <summary>
        /// Create a new AI suggestion record
        /// </summary>
        Task<AiSuggestion> CreateAsync(AiSuggestion suggestion);

        /// <summary>
        /// Get suggestion by ID
        /// </summary>
        Task<AiSuggestion?> GetByIdAsync(Guid id);

        /// <summary>
        /// Get all suggestions for a specific user
        /// </summary>
        Task<List<AiSuggestion>> GetByUserIdAsync(Guid userId, int? limit = null);

        /// <summary>
        /// Get all suggestions for a specific game
        /// </summary>
        Task<List<AiSuggestion>> GetByGameIdAsync(Guid gameId);

        /// <summary>
        /// Get total points spent on suggestions by user
        /// </summary>
        Task<int> GetTotalPointsSpentAsync(Guid userId);

        /// <summary>
        /// Get suggestion count for a user
        /// </summary>
        Task<int> GetUserSuggestionCountAsync(Guid userId);
    }
}
