using robot_chess_api.Models;

namespace robot_chess_api.Repositories
{
    public interface ISavedStateRepository
    {
        Task<SavedState?> GetLatestByGameIdAsync(Guid gameId);
        Task<SavedState> CreateAsync(SavedState savedState);
        Task<List<SavedState>> GetByPlayerIdAsync(Guid playerId);
        Task<List<SavedState>> GetAllByGameIdAsync(Guid gameId);
    }
}
