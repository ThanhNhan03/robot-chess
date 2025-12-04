using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingPuzzlesController : ControllerBase
    {
        private readonly ILogger<TrainingPuzzlesController> _logger;
        private readonly ITrainingPuzzleService _puzzleService;

        public TrainingPuzzlesController(
            ILogger<TrainingPuzzlesController> logger,
            ITrainingPuzzleService puzzleService)
        {
            _logger = logger;
            _puzzleService = puzzleService;
        }

        /// <summary>
        /// Get all training puzzles
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainingPuzzleDto>>> GetAllPuzzles()
        {
            try
            {
                var puzzles = await _puzzleService.GetAllPuzzlesAsync();
                return Ok(puzzles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all puzzles");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get puzzles by difficulty
        /// </summary>
        [HttpGet("difficulty/{difficulty}")]
        public async Task<ActionResult<IEnumerable<TrainingPuzzleDto>>> GetPuzzlesByDifficulty(string difficulty)
        {
            try
            {
                var puzzles = await _puzzleService.GetPuzzlesByDifficultyAsync(difficulty);
                return Ok(puzzles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting puzzles by difficulty {difficulty}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a random puzzle by difficulty
        /// </summary>
        [HttpGet("random/{difficulty}")]
        public async Task<ActionResult<TrainingPuzzleDto>> GetRandomPuzzle(string difficulty)
        {
            try
            {
                var puzzle = await _puzzleService.GetRandomPuzzleByDifficultyAsync(difficulty);
                if (puzzle == null)
                {
                    return NotFound(new { message = $"No puzzle found for difficulty {difficulty}" });
                }
                return Ok(puzzle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting random puzzle for difficulty {difficulty}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get puzzle by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TrainingPuzzleDto>> GetPuzzleById(Guid id)
        {
            try
            {
                var puzzle = await _puzzleService.GetPuzzleByIdAsync(id);
                if (puzzle == null)
                {
                    return NotFound(new { message = $"Puzzle with ID {id} not found" });
                }
                return Ok(puzzle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting puzzle {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Initialize hardcoded puzzles (development only)
        /// </summary>
        [HttpPost("initialize")]
        public async Task<ActionResult> InitializePuzzles()
        {
            try
            {
                await _puzzleService.InitializeHardcodedPuzzlesAsync();
                return Ok(new { message = "Puzzles initialized successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing puzzles");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
