using robot_chess_api.DTOs;
using robot_chess_api.Models;
using robot_chess_api.Repositories;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Services.Implement
{
    public class TrainingPuzzleService : ITrainingPuzzleService
    {
        private readonly ITrainingPuzzleRepository _puzzleRepository;
        private readonly ILogger<TrainingPuzzleService> _logger;

        public TrainingPuzzleService(
            ITrainingPuzzleRepository puzzleRepository,
            ILogger<TrainingPuzzleService> logger)
        {
            _puzzleRepository = puzzleRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<TrainingPuzzleDto>> GetAllPuzzlesAsync()
        {
            var puzzles = await _puzzleRepository.GetAllAsync();
            return puzzles.Select(MapToDto);
        }

        public async Task<IEnumerable<TrainingPuzzleDto>> GetPuzzlesByDifficultyAsync(string difficulty)
        {
            var puzzles = await _puzzleRepository.GetByDifficultyAsync(difficulty);
            return puzzles.Select(MapToDto);
        }

        public async Task<TrainingPuzzleDto?> GetRandomPuzzleByDifficultyAsync(string difficulty)
        {
            var puzzles = await _puzzleRepository.GetByDifficultyAsync(difficulty);
            var puzzleList = puzzles.ToList();
            
            if (puzzleList.Count == 0)
                return null;

            var random = new Random();
            var randomPuzzle = puzzleList[random.Next(puzzleList.Count)];
            return MapToDto(randomPuzzle);
        }

        public async Task<TrainingPuzzleDto?> GetPuzzleByIdAsync(Guid id)
        {
            var puzzle = await _puzzleRepository.GetByIdAsync(id);
            return puzzle == null ? null : MapToDto(puzzle);
        }

        public async Task InitializeHardcodedPuzzlesAsync()
        {
            // Check if puzzles already exist
            var count = await _puzzleRepository.CountAsync();
            if (count > 0)
            {
                _logger.LogInformation("Puzzles already initialized. Clearing existing puzzles...");
                await _puzzleRepository.DeleteAllAsync();
            }

            // Hardcoded 10 chess puzzles with different difficulties
            var hardcodedPuzzles = new List<TrainingPuzzle>
            {
                // Easy puzzles (1-3)
                new TrainingPuzzle
                {
                    Id = Guid.NewGuid(),
                    FenStr = "r1bqkb1r/pppp1ppp/2n2n2/4p2Q/2B1P3/8/PPPP1PPP/RNB1K1NR w KQkq - 0 1",
                    SolutionMove = "Qxf7#",
                    Difficulty = "easy",
                    CreatedAt = DateTime.UtcNow
                },
                new TrainingPuzzle
                {
                    Id = Guid.NewGuid(),
                    FenStr = "r1bqk2r/pppp1ppp/2n2n2/2b1p3/2B1P3/3P1N2/PPP2PPP/RNBQK2R w KQkq - 0 1",
                    SolutionMove = "Bxf7+",
                    Difficulty = "easy",
                    CreatedAt = DateTime.UtcNow
                },
                new TrainingPuzzle
                {
                    Id = Guid.NewGuid(),
                    FenStr = "r1bqkbnr/pppp1ppp/2n5/4p3/2B1P3/5Q2/PPPP1PPP/RNB1K1NR w KQkq - 0 1",
                    SolutionMove = "Qxf7#",
                    Difficulty = "easy",
                    CreatedAt = DateTime.UtcNow
                },

                // Medium puzzles (4-6)
                new TrainingPuzzle
                {
                    Id = Guid.NewGuid(),
                    FenStr = "5r1k/1b2Nppp/8/2R5/4Q3/8/5PPP/6K1 w - - 0 1",
                    SolutionMove = "Qh4",
                    Difficulty = "medium",
                    CreatedAt = DateTime.UtcNow
                },
                new TrainingPuzzle
                {
                    Id = Guid.NewGuid(),
                    FenStr = "r1bq1rk1/ppp2ppp/2np1n2/2b1p3/2B1P3/2NP1N2/PPP2PPP/R1BQ1RK1 w - - 0 1",
                    SolutionMove = "Bxf7+",
                    Difficulty = "medium",
                    CreatedAt = DateTime.UtcNow
                },
                new TrainingPuzzle
                {
                    Id = Guid.NewGuid(),
                    FenStr = "r2qkb1r/pp2nppp/3p4/2pNN1B1/2BnP3/3P4/PPP2PPP/R2bK2R w KQkq - 0 1",
                    SolutionMove = "Nf6+",
                    Difficulty = "medium",
                    CreatedAt = DateTime.UtcNow
                },

                // Hard puzzles (7-10)
                new TrainingPuzzle
                {
                    Id = Guid.NewGuid(),
                    FenStr = "r1b2rk1/ppp2ppp/8/4N3/1b1pn3/8/PPP2PPP/RNBQR1K1 w - - 0 1",
                    SolutionMove = "Qxd4",
                    Difficulty = "hard",
                    CreatedAt = DateTime.UtcNow
                },
                new TrainingPuzzle
                {
                    Id = Guid.NewGuid(),
                    FenStr = "r4rk1/1b3ppp/pq1bpn2/1p6/3NP3/PBN2P2/1P1Q2PP/2R2RK1 w - - 0 1",
                    SolutionMove = "Nf5",
                    Difficulty = "hard",
                    CreatedAt = DateTime.UtcNow
                },
                new TrainingPuzzle
                {
                    Id = Guid.NewGuid(),
                    FenStr = "r2q1rk1/pb2bppp/1p2pn2/3pN3/3P4/P2B1N2/1P3PPP/R2QR1K1 w - - 0 1",
                    SolutionMove = "Nxf7",
                    Difficulty = "hard",
                    CreatedAt = DateTime.UtcNow
                },
                new TrainingPuzzle
                {
                    Id = Guid.NewGuid(),
                    FenStr = "r1b1r1k1/pp1n1pbp/1qp3p1/3p4/1P1P4/P1N1PN2/4BPPP/R2QR1K1 w - - 0 1",
                    SolutionMove = "Bh5",
                    Difficulty = "hard",
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Save all puzzles to database
            foreach (var puzzle in hardcodedPuzzles)
            {
                await _puzzleRepository.CreateAsync(puzzle);
            }

            _logger.LogInformation($"Successfully initialized {hardcodedPuzzles.Count} training puzzles");
        }

        private TrainingPuzzleDto MapToDto(TrainingPuzzle puzzle)
        {
            return new TrainingPuzzleDto
            {
                Id = puzzle.Id,
                FenStr = puzzle.FenStr,
                SolutionMove = puzzle.SolutionMove,
                Difficulty = puzzle.Difficulty,
                CreatedAt = puzzle.CreatedAt
            };
        }
    }
}
