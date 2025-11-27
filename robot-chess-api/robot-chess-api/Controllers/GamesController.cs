using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Services.Interface;
using System.Security.Claims;

namespace robot_chess_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly ILogger<GamesController> _logger;
        private readonly IGameService _gameService;

        public GamesController(ILogger<GamesController> logger, IGameService gameService)
        {
            _logger = logger;
            _gameService = gameService;
        }

        /// <summary>
        /// Get all available game types
        /// </summary>
        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<GameTypeDto>>> GetGameTypes()
        {
            try
            {
                var gameTypes = await _gameService.GetAllGameTypesAsync();
                return Ok(gameTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game types");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        /// <param name="request">Game start request with game type and difficulty</param>
        [HttpPost("start")]
        [Authorize]
        public async Task<ActionResult<StartGameResponseDto>> StartGame([FromBody] StartGameRequestDto request)
        {
            try
            {
                // Get player ID from token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid playerId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var response = await _gameService.StartGameAsync(request, playerId);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid start game request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting game");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Resume an existing game
        /// </summary>
        /// <param name="request">Resume game request with game ID</param>
        [HttpPost("resume")]
        [Authorize]
        public async Task<ActionResult<StartGameResponseDto>> ResumeGame([FromBody] ResumeGameRequestDto request)
        {
            try
            {
                var response = await _gameService.ResumeGameAsync(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid resume game request");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot resume game");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming game");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get game by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GameDto>> GetGameById(Guid id)
        {
            try
            {
                var game = await _gameService.GetGameByIdAsync(id);
                if (game == null)
                {
                    return NotFound(new { message = $"Game with ID {id} not found" });
                }
                return Ok(game);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting game {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all games for a player
        /// </summary>
        [HttpGet("player/{playerId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetPlayerGames(Guid playerId)
        {
            try
            {
                var games = await _gameService.GetPlayerGamesAsync(playerId);
                return Ok(games);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting games for player {playerId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}

