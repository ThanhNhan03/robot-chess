using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories
{
    public class TrainingPuzzleRepository : ITrainingPuzzleRepository
    {
        private readonly PostgresContext _context;

        public TrainingPuzzleRepository(PostgresContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TrainingPuzzle>> GetAllAsync()
        {
            return await _context.TrainingPuzzles
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TrainingPuzzle>> GetByDifficultyAsync(string difficulty)
        {
            return await _context.TrainingPuzzles
                .Where(p => p.Difficulty == difficulty)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<TrainingPuzzle?> GetByIdAsync(Guid id)
        {
            return await _context.TrainingPuzzles
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<TrainingPuzzle> CreateAsync(TrainingPuzzle puzzle)
        {
            _context.TrainingPuzzles.Add(puzzle);
            await _context.SaveChangesAsync();
            return puzzle;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.TrainingPuzzles
                .AnyAsync(p => p.Id == id);
        }

        public async Task<int> CountAsync()
        {
            return await _context.TrainingPuzzles.CountAsync();
        }

        public async Task DeleteAllAsync()
        {
            var allPuzzles = await _context.TrainingPuzzles.ToListAsync();
            _context.TrainingPuzzles.RemoveRange(allPuzzles);
            await _context.SaveChangesAsync();
        }
    }
}
