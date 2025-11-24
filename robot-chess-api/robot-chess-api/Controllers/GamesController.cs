using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Services.Interface;
using System.Security.Claims;

namespace robot_chess_api.Controllers
{
    [Route("api/[controller]")]
    // [ApiController]
    // [Authorize] // Uncomment when Auth is fully ready on client
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly ILogger<GamesController> _logger;

        public GamesController(IGameService gameService, ILogger<GamesController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        [HttpPost("start")]
        public async Task<ActionResult<GameDto>> StartGame([FromBody] StartGameRequest request)
        {
            try
            {
                // Get User ID from Token or use null if not authenticated
                Guid? userId = null;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid parsedId))
                {
                    userId = parsedId;
                }
                
                // If userId is empty, we might want to handle it. 
                // For now, let's proceed. If it fails, it fails.
                
                var game = await _gameService.StartGameAsync(request, userId);
                return Ok(game);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting game");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/stop")]
        public async Task<ActionResult> StopGame(Guid id)
        {
            try
            {
                var result = await _gameService.StopGameAsync(id);
                if (!result)
                {
                    return NotFound($"Game with ID {id} not found");
                }
                return Ok(new { message = "Game stopped successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error stopping game {id}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<GameDto>> GetGame(Guid id)
        {
            var game = await _gameService.GetGameAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return Ok(game);
        }
    }
}
