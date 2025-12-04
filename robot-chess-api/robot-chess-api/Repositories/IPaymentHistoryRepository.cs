using robot_chess_api.DTOs;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

public interface IPaymentHistoryRepository
{
    /// <summary>
    /// Get all payment records with optional filtering
    /// </summary>
    Task<IEnumerable<PaymentHistory>> GetAllAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null);
    
    /// <summary>
    /// Get payment records by user ID
    /// </summary>
    Task<IEnumerable<PaymentHistory>> GetByUserIdAsync(Guid userId);
    
    /// <summary>
    /// Get payment by transaction ID
    /// </summary>
    Task<PaymentHistory?> GetByTransactionIdAsync(string transactionId);
    
    /// <summary>
    /// Get payment by order code
    /// </summary>
    Task<PaymentHistory?> GetByOrderCodeAsync(string orderCode);
    
    /// <summary>
    /// Get payment statistics for admin dashboard
    /// </summary>
    Task<PaymentStatisticsDto> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Create new payment record
    /// </summary>
    Task<PaymentHistory> CreateAsync(PaymentHistory payment);
    
    /// <summary>
    /// Update payment record
    /// </summary>
    Task<PaymentHistory> UpdateAsync(PaymentHistory payment);
}
