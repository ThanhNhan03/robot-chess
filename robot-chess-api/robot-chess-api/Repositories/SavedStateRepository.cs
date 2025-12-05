using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories
{
    public class SavedStateRepository : ISavedStateRepository
    {
        private readonly PostgresContext _context;
        private readonly ILogger<SavedStateRepository> _logger;

        public SavedStateRepository(PostgresContext context, ILogger<SavedStateRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SavedState?> GetLatestByGameIdAsync(Guid gameId)
        {
            try
            {
                return await _context.SavedStates
                    .Where(s => s.GameId == gameId)
                    .OrderByDescending(s => s.SavedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting latest saved state for game {gameId}");
                throw;
            }
        }

        public async Task<SavedState> CreateAsync(SavedState savedState)
        {
            try
            {
                _context.SavedStates.Add(savedState);
                await _context.SaveChangesAsync();
                return savedState;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating saved state");
                throw;
            }
        }

        public async Task<List<SavedState>> GetByPlayerIdAsync(Guid playerId)
        {
            try
            {
                return await _context.SavedStates
                    .Where(s => s.PlayerId == playerId)
                    .OrderByDescending(s => s.SavedAt)
                    .Take(20) // Limit to last 20 saved states
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting saved states for player {playerId}");
                throw;
            }
        }

        public async Task<List<SavedState>> GetAllByGameIdAsync(Guid gameId)
        {
            try
            {
                return await _context.SavedStates
                    .Where(s => s.GameId == gameId)
                    .OrderByDescending(s => s.SavedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting all saved states for game {gameId}");
                throw;
            }
        }
    }
}
