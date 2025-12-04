using robot_chess_api.DTOs;

namespace robot_chess_api.Services.Interface;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreatePaymentAsync(Guid userId, int packageId);
    Task<PaymentStatusDto> CheckPaymentStatusAsync(string transactionId);
    Task<bool> ProcessWebhookAsync(PaymentWebhookDto webhookData);
}
