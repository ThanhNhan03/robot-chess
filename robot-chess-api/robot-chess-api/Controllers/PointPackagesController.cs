using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PointPackagesController : ControllerBase
{
    private readonly IPointPackageService _pointPackageService;
    private readonly ILogger<PointPackagesController> _logger;

    public PointPackagesController(
        IPointPackageService pointPackageService,
        ILogger<PointPackagesController> logger)
    {
        _pointPackageService = pointPackageService;
        _logger = logger;
    }

    /// <summary>
    /// Get all point packages (Admin)
    /// </summary>
    [HttpGet("admin")]
    public async Task<ActionResult<List<PointPackageDto>>> GetAllPackages()
    {
        try
        {
            var packages = await _pointPackageService.GetAllPackagesAsync();
            return Ok(packages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all point packages");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get active point packages (Public)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PointPackageDto>>> GetActivePackages()
    {
        try
        {
            var packages = await _pointPackageService.GetActivePackagesAsync();
            return Ok(packages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active point packages");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get point package by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PointPackageDto>> GetPackageById(int id)
    {
        try
        {
            var package = await _pointPackageService.GetPackageByIdAsync(id);
            if (package == null)
            {
                return NotFound($"Point package with ID {id} not found");
            }
            return Ok(package);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting point package {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create new point package (Admin)
    /// </summary>
    [HttpPost("admin")]
    public async Task<ActionResult<PointPackageDto>> CreatePackage([FromBody] CreatePointPackageDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var package = await _pointPackageService.CreatePackageAsync(dto);
            return CreatedAtAction(nameof(GetPackageById), new { id = package.Id }, package);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating point package");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update point package (Admin)
    /// </summary>
    [HttpPut("admin/{id}")]
    public async Task<ActionResult<PointPackageDto>> UpdatePackage(int id, [FromBody] UpdatePointPackageDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var package = await _pointPackageService.UpdatePackageAsync(id, dto);
            return Ok(package);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, $"Point package {id} not found");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating point package {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete point package (Admin)
    /// </summary>
    [HttpDelete("admin/{id}")]
    public async Task<IActionResult> DeletePackage(int id)
    {
        try
        {
            var result = await _pointPackageService.DeletePackageAsync(id);
            if (!result)
            {
                return NotFound($"Point package with ID {id} not found");
            }
            return Ok(new { Message = "Point package deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting point package {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Purchase point package
    /// </summary>
    [HttpPost("purchase")]
    public async Task<IActionResult> PurchasePackage([FromBody] PurchasePointPackageDto dto, [FromQuery] Guid userId, [FromQuery] string transactionId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message) = await _pointPackageService.PurchasePackageAsync(userId, dto.PackageId, transactionId);
            
            if (!success)
            {
                return BadRequest(new { Message = message });
            }

            return Ok(new { Message = message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purchasing point package");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Use points for AI suggestions or other services
    /// </summary>
    [HttpPost("use")]
    public async Task<IActionResult> UsePoints([FromBody] UsePointsDto dto, [FromQuery] Guid userId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message) = await _pointPackageService.UsePointsAsync(userId, dto.Amount, dto.Description);
            
            if (!success)
            {
                return BadRequest(new { Message = message });
            }

            return Ok(new { Message = message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using points");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user's point transaction history
    /// </summary>
    [HttpGet("transactions/{userId}")]
    public async Task<ActionResult<List<PointTransactionDto>>> GetUserTransactions(Guid userId)
    {
        try
        {
            var transactions = await _pointPackageService.GetUserTransactionsAsync(userId);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting transactions for user {userId}");
            return StatusCode(500, "Internal server error");
        }
    }
}
