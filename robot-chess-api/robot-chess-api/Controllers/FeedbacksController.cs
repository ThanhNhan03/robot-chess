using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Services.Interface;
using System.Security.Claims;

namespace robot_chess_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FeedbacksController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly ILogger<FeedbacksController> _logger;

    public FeedbacksController(IFeedbackService feedbackService, ILogger<FeedbacksController> logger)
    {
        _feedbackService = feedbackService;
        _logger = logger;
    }

    /// <summary>
    /// Get all feedbacks (Admin only)
    /// </summary>
    [HttpGet("admin")]
    [Helpers.Authorize]
    public async Task<ActionResult<List<FeedbackDto>>> GetAllFeedbacks()
    {
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "admin")
            {
                return Forbid();
            }

            var feedbacks = await _feedbackService.GetAllFeedbacksAsync();
            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all feedbacks");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get feedback by ID (Admin or owner)
    /// </summary>
    [HttpGet("{id}")]
    [Helpers.Authorize]
    public async Task<ActionResult<FeedbackDto>> GetFeedbackById(Guid id)
    {
        try
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound(new { Message = $"Feedback with ID {id} not found" });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Only admin or owner can view specific feedback
            if (userRole != "admin" && feedback.UserId != userId)
            {
                return Forbid();
            }

            return Ok(feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting feedback {id}");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get current user's feedbacks
    /// </summary>
    [HttpGet("my-feedbacks")]
    [Helpers.Authorize]
    public async Task<ActionResult<List<FeedbackDto>>> GetMyFeedbacks()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var feedbacks = await _feedbackService.GetFeedbacksByUserIdAsync(userId);
            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user feedbacks");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create new feedback
    /// </summary>
    [HttpPost]
    [Helpers.Authorize]
    public async Task<ActionResult<FeedbackDto>> CreateFeedback([FromBody] CreateFeedbackDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var feedback = await _feedbackService.CreateFeedbackAsync(userId, dto);
            
            return CreatedAtAction(nameof(GetFeedbackById), new { id = feedback.Id }, feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feedback");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update feedback (Owner only)
    /// </summary>
    [HttpPut("{id}")]
    [Helpers.Authorize]
    public async Task<ActionResult<FeedbackDto>> UpdateFeedback(Guid id, [FromBody] UpdateFeedbackDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var feedback = await _feedbackService.UpdateFeedbackAsync(id, userId, dto);
            
            return Ok(feedback);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating feedback {id}");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete feedback (Admin or owner)
    /// </summary>
    [HttpDelete("{id}")]
    [Helpers.Authorize]
    public async Task<IActionResult> DeleteFeedback(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var isAdmin = userRole == "admin";

            var result = await _feedbackService.DeleteFeedbackAsync(id, userId, isAdmin);
            
            if (!result)
            {
                return NotFound(new { Message = $"Feedback with ID {id} not found" });
            }

            return Ok(new { Message = "Feedback deleted successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting feedback {id}");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get feedbacks by user ID (Admin only)
    /// </summary>
    [HttpGet("user/{userId}")]
    [Helpers.Authorize]
    public async Task<ActionResult<List<FeedbackDto>>> GetFeedbacksByUserId(Guid userId)
    {
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "admin")
            {
                return Forbid();
            }

            var feedbacks = await _feedbackService.GetFeedbacksByUserIdAsync(userId);
            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting feedbacks for user {userId}");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }
}
