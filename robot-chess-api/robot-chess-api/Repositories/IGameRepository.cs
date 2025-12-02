using robot_chess_api.Models;

namespace robot_chess_api.Repositories
{
    public interface IGameRepository
    {
        Task<Game?> GetByIdAsync(Guid id);
        Task<IEnumerable<Game>> GetByPlayerIdAsync(Guid playerId);
        Task<Game> CreateAsync(Game game);
        Task<Game> UpdateAsync(Game game);
        Task<GameType?> GetGameTypeByCodeAsync(string code);
        Task<IEnumerable<GameType>> GetAllGameTypesAsync();
    }
}
