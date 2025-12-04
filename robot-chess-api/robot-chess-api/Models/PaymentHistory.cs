using System;
using System.Collections.Generic;

namespace robot_chess_api.Models;

public partial class PaymentHistory
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? PackageId { get; set; }

    // Navigation properties
    public virtual AppUser? User { get; set; }
    
    public virtual PointPackage? Package { get; set; }
    
    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();
}
