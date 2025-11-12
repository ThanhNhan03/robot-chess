using System;
using System.Collections.Generic;

namespace robot_chest_api.Data;

public partial class PaymentHistory
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User1? User { get; set; }
}
