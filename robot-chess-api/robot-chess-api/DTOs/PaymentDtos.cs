using System.ComponentModel.DataAnnotations;

namespace robot_chess_api.DTOs;

public class CreatePaymentDto
{
    [Required]
    public int PackageId { get; set; }
}

public class PaymentResponseDto
{
    public string PaymentUrl { get; set; } = null!;
    public string TransactionId { get; set; } = null!;
    public decimal Amount { get; set; }
    public string QrCodeUrl { get; set; } = null!;
}

public class PaymentStatusDto
{
    public string TransactionId { get; set; } = null!;
    public string Status { get; set; } = null!; // pending, success, failed
    public decimal Amount { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class PaymentWebhookDto
{
    public string Code { get; set; } = null!;
    public string Desc { get; set; } = null!;
    public bool Success { get; set; }
    public PaymentWebhookData? Data { get; set; }
    public string? Signature { get; set; }
}

public class PaymentWebhookData
{
    public string OrderCode { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string Reference { get; set; } = null!;
    public string TransactionDateTime { get; set; } = null!;
}

public class PaymentHistoryDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? TransactionId { get; set; }
    public string? OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? PackageId { get; set; }
    public PointPackageDto? Package { get; set; }
}
