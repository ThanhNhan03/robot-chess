using System.ComponentModel.DataAnnotations;

namespace robot_chess_api.DTOs;

public class PointPackageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Points { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePointPackageDto
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Points is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Points must be greater than 0")]
    public int Points { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdatePointPackageDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Points must be greater than 0")]
    public int? Points { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal? Price { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}

public class PurchasePointPackageDto
{
    [Required]
    public int PackageId { get; set; }
}

public class UsePointsDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public int Amount { get; set; }

    [Required]
    public string Description { get; set; } = null!;
}

public class PointTransactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Amount { get; set; }
    public string TransactionType { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? RelatedPaymentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentStatisticsDto
{
    public int TotalPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public int PendingPayments { get; set; }
    public int FailedPayments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal ThisMonthRevenue { get; set; }
}

public class AdjustPointsDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [Range(-1000000, 1000000, ErrorMessage = "Amount must be between -1000000 and 1000000")]
    public int Amount { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = null!;
}

