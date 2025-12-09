using robot_chess_api.DTOs;
using robot_chess_api.Models;

namespace robot_chess_api.Services.Interface;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreatePaymentAsync(Guid userId, int packageId);
    Task<PaymentStatusDto> CheckPaymentStatusAsync(string transactionId);
    Task<bool> ProcessWebhookAsync(PaymentWebhookDto webhookData);
    Task<IEnumerable<PaymentHistory>> GetAllPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null);
    Task<PaymentStatisticsDto> GetPaymentStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<PaymentHistory>> GetUserPaymentHistoryAsync(Guid userId);
}
