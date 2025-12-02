using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly PostgresContext _context;

        public GameRepository(PostgresContext context)
        {
            _context = context;
        }

        public async Task<Game?> GetByIdAsync(Guid id)
        {
            return await _context.Games
                .Include(g => g.GameType)
                .Include(g => g.Player)
                .Include(g => g.Puzzle)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<Game>> GetByPlayerIdAsync(Guid playerId)
        {
            return await _context.Games
                .Include(g => g.GameType)
                .Where(g => g.PlayerId == playerId)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        public async Task<Game> CreateAsync(Game game)
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task<Game> UpdateAsync(Game game)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task<GameType?> GetGameTypeByCodeAsync(string code)
        {
            return await _context.GameTypes
                .FirstOrDefaultAsync(gt => gt.Code == code);
        }

        public async Task<IEnumerable<GameType>> GetAllGameTypesAsync()
        {
            return await _context.GameTypes.ToListAsync();
        }
    }
}
