using robot_chess_api.Models;

namespace robot_chess_api.Repositories
{
    public interface ITrainingPuzzleRepository
    {
        Task<IEnumerable<TrainingPuzzle>> GetAllAsync();
        Task<IEnumerable<TrainingPuzzle>> GetByDifficultyAsync(string difficulty);
        Task<TrainingPuzzle?> GetByIdAsync(Guid id);
        Task<TrainingPuzzle> CreateAsync(TrainingPuzzle puzzle);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountAsync();
        Task DeleteAllAsync();
    }
}
