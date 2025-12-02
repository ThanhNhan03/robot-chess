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
        /// Update game result (win/lose/draw) and total moves
        /// NOTE: This must come BEFORE [HttpGet("{id}")] to avoid route conflicts
        /// </summary>
        [HttpPut("{gameId}/result")]
        [Authorize]
        public async Task<ActionResult<UpdateGameResultResponseDto>> UpdateGameResult(
            Guid gameId,
            [FromBody] UpdateGameResultRequestDto request)
        {
            try
            {
                // Ensure gameId in route matches request body
                if (request.GameId == Guid.Empty)
                {
                    request.GameId = gameId;
                }
                else if (request.GameId != gameId)
                {
                    return BadRequest(new { message = "Game ID in route does not match request body" });
                }

                var result = await _gameService.UpdateGameResultAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Invalid request for updating game {gameId} result");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating game {gameId} result");
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

        /// <summary>
        /// Request AI to verify board setup for a game
        /// </summary>
        /// <param name="request">Request with game ID to verify</param>
        [HttpPost("verify-board-setup")]
        [Authorize]
        public async Task<ActionResult<BoardSetupStatusDto>> VerifyBoardSetup([FromBody] VerifyBoardSetupRequestDto request)
        {
            try
            {
                var result = await _gameService.VerifyBoardSetupAsync(request.GameId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Invalid game ID: {request.GameId}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying board setup for game {request.GameId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Save a single move to the database
        /// </summary>
        [HttpPost("moves")]
        [Authorize]
        public async Task<ActionResult<GameMoveDto>> SaveMove([FromBody] CreateGameMoveDto moveDto)
        {
            try
            {
                var savedMove = await _gameService.SaveMoveAsync(moveDto);
                return Ok(savedMove);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid move data");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving move");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Save multiple moves in batch
        /// </summary>
        [HttpPost("moves/batch")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GameMoveDto>>> SaveMoves([FromBody] SaveMovesRequestDto request)
        {
            try
            {
                var savedMoves = await _gameService.SaveMovesAsync(request);
                return Ok(savedMoves);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid moves data");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving moves");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all moves for a game
        /// </summary>
        [HttpGet("{gameId}/moves")]
        public async Task<ActionResult<IEnumerable<GameMoveDto>>> GetGameMoves(Guid gameId)
        {
            try
            {
                var moves = await _gameService.GetGameMovesAsync(gameId);
                return Ok(moves);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting moves for game {gameId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get moves for a game within a specific range
        /// </summary>
        [HttpPost("moves/range")]
        public async Task<ActionResult<IEnumerable<GameMoveDto>>> GetGameMovesRange([FromBody] GetMovesRequestDto request)
        {
            try
            {
                var moves = await _gameService.GetGameMovesRangeAsync(request);
                return Ok(moves);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting move range for game {request.GameId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// TEST ONLY: Start a training puzzle game without auth or database
        /// Fixed puzzle FEN: "5r1k/1b2Nppp/8/2R5/4Q3/8/5PPP/6K1 w - - 0 1"
        /// </summary>
        [HttpPost("test-puzzle")]
        public async Task<ActionResult> TestPuzzle()
        {
            try
            {
                var gameId = Guid.NewGuid();
                var requestId = Guid.NewGuid();
                var puzzleFen = "5r1k/1b2Nppp/8/2R5/4Q3/8/5PPP/6K1 w - - 0 1";
                var difficulty = "medium";

                // Send command to AI via TCP Server
                var aiMessage = new
                {
                    Type = "ai_request",
                    Command = "start_game",
                    Payload = new
                    {
                        game_id = gameId.ToString(),
                        status = "start",
                        game_type = "training_puzzle",
                        difficulty = difficulty,
                        puzzle_fen = puzzleFen
                    }
                };

                var httpClient = new HttpClient();
                var tcpServerUrl = "http://localhost:5000"; // TCP Server URL

                var response = await httpClient.PostAsJsonAsync($"{tcpServerUrl}/internal/ai-command", aiMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to send test puzzle command to TCP Server: {response.StatusCode}");
                    return StatusCode(500, new { message = "Failed to communicate with TCP Server" });
                }

                _logger.LogInformation($"Test puzzle command sent. Game ID: {gameId}, FEN: {puzzleFen}");

                return Ok(new
                {
                    game_id = gameId,
                    request_id = requestId,
                    game_type = "training_puzzle",
                    puzzle_fen = puzzleFen,
                    difficulty = difficulty,
                    status = "sent",
                    message = "Test puzzle command sent to AI successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test puzzle command");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}

 