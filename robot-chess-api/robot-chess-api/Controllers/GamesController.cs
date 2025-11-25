using Microsoft.AspNetCore.Mvc;

namespace robot_chess_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly ILogger<GamesController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _tcpServerUrl = "http://localhost:5000";

        public GamesController(ILogger<GamesController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Start a new game - sends message to AI via TCP server
        /// </summary>
        /// <param name="difficulty">Game difficulty (easy, medium, hard)</param>
        /// <returns>Request ID for tracking</returns>
        [HttpPost("start")]
        public async Task<ActionResult> StartGame([FromBody] StartGameRequest request)
        {
            try
            {
                var requestId = Guid.NewGuid();
                
                // Construct message for AI
                var aiMessage = new
                {
                    type = "ai_request",
                    request_id = requestId.ToString(),
                    command = "start_game",
                    payload = new
                    {
                        status = "start",
                        difficulty = request.Difficulty ?? "medium"
                    }
                };

                // Send to TCP Server
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.PostAsJsonAsync($"{_tcpServerUrl}/internal/ai-command", aiMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to send start game command to TCP Server: {response.StatusCode}");
                    return StatusCode(500, new { message = "Failed to communicate with game server" });
                }

                _logger.LogInformation($"Start game command sent successfully. Request ID: {requestId}");

                return Ok(new
                {
                    request_id = requestId,
                    message = "Game start command sent to AI",
                    difficulty = request.Difficulty ?? "medium"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting game");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Resume an existing game - sends message to AI via TCP server
        /// </summary>
        /// <param name="request">Resume game request with FEN and difficulty</param>
        /// <returns>Request ID for tracking</returns>
        [HttpPost("resume")]
        public async Task<ActionResult> ResumeGame([FromBody] ResumeGameRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Fen))
                {
                    return BadRequest(new { message = "FEN string is required to resume game" });
                }

                var requestId = Guid.NewGuid();

                // Construct message for AI
                var aiMessage = new
                {
                    type = "ai_request",
                    request_id = requestId.ToString(),
                    command = "resume_game",
                    payload = new
                    {
                        status = "resume",
                        difficulty = request.Difficulty ?? "medium",
                        fen = request.Fen
                    }
                };

                // Send to TCP Server
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.PostAsJsonAsync($"{_tcpServerUrl}/internal/ai-command", aiMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to send resume game command to TCP Server: {response.StatusCode}");
                    return StatusCode(500, new { message = "Failed to communicate with game server" });
                }

                _logger.LogInformation($"Resume game command sent successfully. Request ID: {requestId}");

                return Ok(new
                {
                    request_id = requestId,
                    message = "Game resume command sent to AI",
                    difficulty = request.Difficulty ?? "medium",
                    fen = request.Fen
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming game");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }

    // DTOs
    public class StartGameRequest
    {
        public string? Difficulty { get; set; }
    }

    public class ResumeGameRequest
    {
        public string Fen { get; set; } = string.Empty;
        public string? Difficulty { get; set; }
    }
}
