using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories
{
    public class GameMoveRepository : IGameMoveRepository
    {
        private readonly PostgresContext _context;

        public GameMoveRepository(PostgresContext context)
        {
            _context = context;
        }

        public async Task<GameMove> CreateAsync(GameMove move)
        {
            _context.GameMoves.Add(move);
            await _context.SaveChangesAsync();
            return move;
        }

        public async Task<IEnumerable<GameMove>> CreateManyAsync(IEnumerable<GameMove> moves)
        {
            _context.GameMoves.AddRange(moves);
            await _context.SaveChangesAsync();
            return moves;
        }

        public async Task<IEnumerable<GameMove>> GetByGameIdAsync(Guid gameId)
        {
            return await _context.GameMoves
                .Where(m => m.GameId == gameId)
                .OrderBy(m => m.MoveNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<GameMove>> GetByGameIdRangeAsync(Guid gameId, int? fromMove, int? toMove)
        {
            var query = _context.GameMoves
                .Where(m => m.GameId == gameId);

            if (fromMove.HasValue)
            {
                query = query.Where(m => m.MoveNumber >= fromMove.Value);
            }

            if (toMove.HasValue)
            {
                query = query.Where(m => m.MoveNumber <= toMove.Value);
            }

            return await query
                .OrderBy(m => m.MoveNumber)
                .ToListAsync();
        }

        public async Task<GameMove?> GetLatestMoveAsync(Guid gameId)
        {
            return await _context.GameMoves
                .Where(m => m.GameId == gameId)
                .OrderByDescending(m => m.MoveNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetMoveCountAsync(Guid gameId)
        {
            return await _context.GameMoves
                .CountAsync(m => m.GameId == gameId);
        }

        public async Task<bool> DeleteByGameIdAsync(Guid gameId)
        {
            var moves = await _context.GameMoves
                .Where(m => m.GameId == gameId)
                .ToListAsync();

            _context.GameMoves.RemoveRange(moves);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
