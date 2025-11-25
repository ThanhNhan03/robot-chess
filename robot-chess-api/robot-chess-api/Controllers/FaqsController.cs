using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FaqsController : ControllerBase
{
    private readonly IFaqService _faqService;
    private readonly ILogger<FaqsController> _logger;

    public FaqsController(IFaqService faqService, ILogger<FaqsController> logger)
    {
        _faqService = faqService;
        _logger = logger;
    }

    /// <summary>
    /// Get all published FAQs (Public)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<FaqDto>>> GetFaqs([FromQuery] string? category)
    {
        try
        {
            if (!string.IsNullOrEmpty(category))
            {
                return Ok(await _faqService.GetFaqsByCategoryAsync(category));
            }
            return Ok(await _faqService.GetAllFaqsAsync(includeUnpublished: false));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FAQs");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all FAQs including unpublished (Admin only)
    /// </summary>
    [HttpGet("admin")]
    // [Authorize(Roles = "admin")] // Uncomment when auth is ready
    public async Task<ActionResult<List<FaqDto>>> GetAllFaqsAdmin()
    {
        try
        {
            return Ok(await _faqService.GetAllFaqsAsync(includeUnpublished: true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin FAQs");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get FAQ by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FaqDto>> GetFaqById(Guid id)
    {
        try
        {
            var faq = await _faqService.GetFaqByIdAsync(id);
            if (faq == null)
            {
                return NotFound($"FAQ with ID {id} not found");
            }
            return Ok(faq);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting FAQ {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create new FAQ (Admin only)
    /// </summary>
    [HttpPost("admin")]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<FaqDto>> CreateFaq([FromBody] CreateFaqDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var faq = await _faqService.CreateFaqAsync(dto);
            return CreatedAtAction(nameof(GetFaqById), new { id = faq.Id }, faq);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating FAQ");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update FAQ (Admin only)
    /// </summary>
    [HttpPut("admin/{id}")]
    // [Authorize(Roles = "admin")]
    public async Task<ActionResult<FaqDto>> UpdateFaq(Guid id, [FromBody] UpdateFaqDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var faq = await _faqService.UpdateFaqAsync(id, dto);
            return Ok(faq);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating FAQ {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete FAQ (Admin only)
    /// </summary>
    [HttpDelete("admin/{id}")]
    // [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteFaq(Guid id)
    {
        try
        {
            var result = await _faqService.DeleteFaqAsync(id);
            if (!result)
            {
                return NotFound($"FAQ with ID {id} not found");
            }
            return Ok(new { Message = "FAQ deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting FAQ {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all FAQ categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<List<string>>> GetCategories()
    {
        try
        {
            return Ok(await _faqService.GetCategoriesAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, "Internal server error");
        }
    }
}
