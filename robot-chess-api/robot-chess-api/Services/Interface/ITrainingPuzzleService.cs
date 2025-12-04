using robot_chess_api.DTOs;

namespace robot_chess_api.Services.Interface
{
    public interface ITrainingPuzzleService
    {
        Task<IEnumerable<TrainingPuzzleDto>> GetAllPuzzlesAsync();
        Task<IEnumerable<TrainingPuzzleDto>> GetPuzzlesByDifficultyAsync(string difficulty);
        Task<TrainingPuzzleDto?> GetRandomPuzzleByDifficultyAsync(string difficulty);
        Task<TrainingPuzzleDto?> GetPuzzleByIdAsync(Guid id);
        Task InitializeHardcodedPuzzlesAsync();
    }
}
