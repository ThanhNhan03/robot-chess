using robot_chess_api.Models;

namespace robot_chess_api.Repositories
{
    public interface IGameMoveRepository
    {
        Task<GameMove> CreateAsync(GameMove move);
        Task<IEnumerable<GameMove>> CreateManyAsync(IEnumerable<GameMove> moves);
        Task<IEnumerable<GameMove>> GetByGameIdAsync(Guid gameId);
        Task<IEnumerable<GameMove>> GetByGameIdRangeAsync(Guid gameId, int? fromMove, int? toMove);
        Task<GameMove?> GetLatestMoveAsync(Guid gameId);
        Task<int> GetMoveCountAsync(Guid gameId);
        Task<bool> DeleteByGameIdAsync(Guid gameId);
    }
}
