using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Models;
using robot_chess_api.Repositories;
using robot_chess_api.Data;
using System.Security.Claims;

namespace robot_chess_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PointTransactionsController : ControllerBase
{
    private readonly IPointTransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly PostgresContext _context;
    private readonly ILogger<PointTransactionsController> _logger;

    public PointTransactionsController(
        IPointTransactionRepository transactionRepository,
        IUserRepository userRepository,
        PostgresContext context,
        ILogger<PointTransactionsController> logger)
    {
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all point transactions (Admin only)
    /// </summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<PointTransaction>>> GetAllTransactions(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? transactionType = null)
    {
        try
        {
            var transactions = await _transactionRepository.GetAllAsync(startDate, endDate, transactionType);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all point transactions");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get point transactions by user ID (Admin only)
    /// </summary>
    [HttpGet("admin/user/{userId}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<PointTransaction>>> GetTransactionsByUser(Guid userId)
    {
        try
        {
            var transactions = await _transactionRepository.GetByUserIdAsync(userId);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting transactions for user {userId}");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get transaction statistics (Admin only)
    /// </summary>
    [HttpGet("admin/statistics")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<Dictionary<string, int>>> GetStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var statistics = await _transactionRepository.GetTransactionStatisticsAsync(startDate, endDate);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction statistics");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Manually adjust user points (Admin only)
    /// </summary>
    [HttpPost("admin/adjust")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<PointTransaction>> AdjustPoints([FromBody] AdjustPointsDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Get admin user ID
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            {
                return Unauthorized(new { error = "Admin not authenticated" });
            }

            // Validate user exists
            var user = await _userRepository.GetUserByIdAsync(dto.UserId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Check if user has enough points for deduction
            if (dto.Amount < 0 && user.PointsBalance < Math.Abs(dto.Amount))
            {
                return BadRequest(new { error = "Insufficient points" });
            }

            // Update user points
            user.PointsBalance += dto.Amount;
            await _userRepository.UpdateUserAsync(user);

            // Get admin info for description
            var admin = await _userRepository.GetUserByIdAsync(adminId);
            var adminName = admin?.Username ?? "Admin";

            // Create transaction record
            var pointTransaction = new PointTransaction
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Amount = dto.Amount,
                TransactionType = "adjustment",
                Description = $"Điều chỉnh bởi {adminName}: {dto.Reason}",
                CreatedAt = DateTime.UtcNow
            };

            var createdTransaction = await _transactionRepository.CreateAsync(pointTransaction);
            
            await transaction.CommitAsync();

            _logger.LogInformation(
                $"Admin {adminId} adjusted {dto.Amount} points for user {dto.UserId}. New balance: {user.PointsBalance}");

            return Ok(new 
            { 
                transaction = new
                {
                    id = createdTransaction.Id,
                    userId = createdTransaction.UserId,
                    amount = createdTransaction.Amount,
                    transactionType = createdTransaction.TransactionType,
                    description = createdTransaction.Description,
                    createdAt = createdTransaction.CreatedAt
                },
                newBalance = user.PointsBalance
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error adjusting user points");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current user's point transactions
    /// </summary>
    [HttpGet("my-transactions")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PointTransaction>>> GetMyTransactions()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var transactions = await _transactionRepository.GetByUserIdAsync(userId);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user transactions");
            return BadRequest(new { error = ex.Message });
        }
    }
}
