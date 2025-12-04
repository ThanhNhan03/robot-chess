using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace robot_chess_api.Models;

[Table("point_transactions")]
public partial class PointTransaction
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("amount")]
    public int Amount { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("transaction_type")]
    public string TransactionType { get; set; } = null!; // 'deposit', 'service_usage', 'adjustment'

    [Column("description")]
    public string? Description { get; set; }

    [Column("related_payment_id")]
    public Guid? RelatedPaymentId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual AppUser? User { get; set; }
    public virtual PaymentHistory? RelatedPayment { get; set; }
}
