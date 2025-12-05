using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Services.Interface;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace robot_chess_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AiSuggestionsController : ControllerBase
    {
        private readonly IAiSuggestionService _suggestionService;
        private readonly ILogger<AiSuggestionsController> _logger;

        public AiSuggestionsController(
            IAiSuggestionService suggestionService,
            ILogger<AiSuggestionsController> logger)
        {
            _suggestionService = suggestionService;
            _logger = logger;
        }

        /// <summary>
        /// Get AI chess move suggestion for current position
        /// Requires 5 points per suggestion
        /// Rate limited: 1 request per 3 seconds
        /// Note: Users can view AI suggestion usage history via /api/PointTransactions/my-transactions
        /// </summary>
        /// <param name="request">Request with FEN position and game ID</param>
        /// <returns>Best move suggestion with evaluation and remaining points</returns>
        [HttpPost("get-suggestion")]
        public async Task<ActionResult<SuggestionResponseDto>> GetSuggestion([FromBody] GetSuggestionRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

                _logger.LogInformation($"User {userId} requesting chess suggestion for game {request.GameId}");

                var response = await _suggestionService.GetSuggestionAsync(userId, request);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request data");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chess suggestion");
                
                if (ex.Message.Contains("đủ điểm") || ex.Message.Contains("Insufficient points"))
                {
                    return BadRequest(new { message = ex.Message, errorCode = "INSUFFICIENT_POINTS" });
                }

                if (ex.Message.Contains("đợi") || ex.Message.Contains("rate limit"))
                {
                    return StatusCode(429, new { message = ex.Message, errorCode = "RATE_LIMITED" });
                }

                return StatusCode(500, new { message = "Error getting chess suggestion", error = ex.Message });
            }
        }

        /// <summary>
        /// Get the cost in points for one AI suggestion
        /// </summary>
        /// <returns>Points required per suggestion (currently 5 points)</returns>
        [HttpGet("cost")]
        [AllowAnonymous]
        public ActionResult<object> GetSuggestionCost()
        {
            var cost = _suggestionService.GetSuggestionCost();
            return Ok(new { 
                cost, 
                description = "Points required per AI chess suggestion",
                rateLimitSeconds = 3
            });
        }
    }
}

