using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Models;
using robot_chess_api.Services.Interface;
using System.Security.Claims;

namespace robot_chess_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Create payment link for purchasing point package
    /// </summary>
    [HttpPost("create")]
    [Authorize]
    public async Task<ActionResult<PaymentResponseDto>> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }
            
            var payment = await _paymentService.CreatePaymentAsync(userId, dto.PackageId);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check payment status
    /// </summary>
    [HttpGet("status/{transactionId}")]
    [Authorize]
    public async Task<ActionResult<PaymentStatusDto>> CheckPaymentStatus(string transactionId)
    {
        try
        {
            var status = await _paymentService.CheckPaymentStatusAsync(transactionId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking payment status for {transactionId}");
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all payments (Admin only)
    /// </summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<PaymentHistory>>> GetAllPayments(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? status = null)
    {
        try
        {
            var payments = await _paymentService.GetAllPaymentsAsync(startDate, endDate, status);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all payments");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get payment statistics (Admin only)
    /// </summary>
    [HttpGet("admin/statistics")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<PaymentStatisticsDto>> GetPaymentStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var statistics = await _paymentService.GetPaymentStatisticsAsync(startDate, endDate);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment statistics");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// PayOS webhook endpoint
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> PayOsWebhook([FromBody] PaymentWebhookDto webhookData)
    {
        try
        {
            _logger.LogInformation($"Received webhook: {System.Text.Json.JsonSerializer.Serialize(webhookData)}");
            
            var result = await _paymentService.ProcessWebhookAsync(webhookData);
            
            if (result)
            {
                return Ok(new { success = true, message = "Webhook processed successfully" });
            }
            
            return BadRequest(new { success = false, message = "Failed to process webhook" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }
}
