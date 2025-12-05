using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace robot_chess_api.Repositories
{
    public class AiSuggestionRepository : IAiSuggestionRepository
    {
        private readonly PostgresContext _context;

        public AiSuggestionRepository(PostgresContext context)
        {
            _context = context;
        }

        public async Task<AiSuggestion> CreateAsync(AiSuggestion suggestion)
        {
            suggestion.Id = Guid.NewGuid();
            suggestion.CreatedAt = DateTime.UtcNow;

            _context.AiSuggestions.Add(suggestion);
            await _context.SaveChangesAsync();

            return suggestion;
        }

        public async Task<AiSuggestion?> GetByIdAsync(Guid id)
        {
            return await _context.AiSuggestions
                .Where(s => s.Id == id)
                .Select(s => new AiSuggestion
                {
                    Id = s.Id,
                    GameId = s.GameId,
                    MoveId = s.MoveId,
                    UserId = s.UserId,
                    SuggestedMove = s.SuggestedMove,
                    FenPosition = s.FenPosition,
                    Evaluation = s.Evaluation,
                    Confidence = s.Confidence,
                    PointsDeducted = s.PointsDeducted,
                    CreatedAt = s.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<AiSuggestion>> GetByUserIdAsync(Guid userId, int? limit = null)
        {
            var query = _context.AiSuggestions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new AiSuggestion
                {
                    Id = s.Id,
                    GameId = s.GameId,
                    MoveId = s.MoveId,
                    UserId = s.UserId,
                    SuggestedMove = s.SuggestedMove,
                    FenPosition = s.FenPosition,
                    Evaluation = s.Evaluation,
                    Confidence = s.Confidence,
                    PointsDeducted = s.PointsDeducted,
                    CreatedAt = s.CreatedAt
                });

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<AiSuggestion>> GetByGameIdAsync(Guid gameId)
        {
            return await _context.AiSuggestions
                .Where(s => s.GameId == gameId)
                .OrderBy(s => s.CreatedAt)
                .Select(s => new AiSuggestion
                {
                    Id = s.Id,
                    GameId = s.GameId,
                    MoveId = s.MoveId,
                    UserId = s.UserId,
                    SuggestedMove = s.SuggestedMove,
                    FenPosition = s.FenPosition,
                    Evaluation = s.Evaluation,
                    Confidence = s.Confidence,
                    PointsDeducted = s.PointsDeducted,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<int> GetTotalPointsSpentAsync(Guid userId)
        {
            var total = await _context.AiSuggestions
                .Where(s => s.UserId == userId && s.PointsDeducted.HasValue)
                .SumAsync(s => s.PointsDeducted ?? 0);

            return total;
        }

        public async Task<int> GetUserSuggestionCountAsync(Guid userId)
        {
            return await _context.AiSuggestions
                .Where(s => s.UserId == userId)
                .CountAsync();
        }
    }
}
