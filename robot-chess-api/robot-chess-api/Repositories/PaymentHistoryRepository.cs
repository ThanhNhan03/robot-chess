using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.DTOs;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

public class PaymentHistoryRepository : IPaymentHistoryRepository
{
    private readonly PostgresContext _context;

    public PaymentHistoryRepository(PostgresContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentHistory>> GetAllAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        string? status = null)
    {
        var query = _context.PaymentHistories
            .AsNoTracking()
            .AsSplitQuery()
            .Select(p => new PaymentHistory
            {
                Id = p.Id,
                UserId = p.UserId,
                TransactionId = p.TransactionId,
                OrderCode = p.OrderCode,
                Amount = p.Amount,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                PackageId = p.PackageId,
                User = p.User != null ? new AppUser
                {
                    Id = p.User.Id,
                    Username = p.User.Username,
                    Email = p.User.Email,
                    FullName = p.User.FullName
                } : null,
                Package = p.Package != null ? new PointPackage
                {
                    Id = p.Package.Id,
                    Name = p.Package.Name,
                    Points = p.Package.Points,
                    Price = p.Package.Price
                } : null
            })
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= endDate.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(p => p.Status == status);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentHistory>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PaymentHistories
            .AsNoTracking()
            .Select(p => new PaymentHistory
            {
                Id = p.Id,
                UserId = p.UserId,
                TransactionId = p.TransactionId,
                OrderCode = p.OrderCode,
                Amount = p.Amount,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                PackageId = p.PackageId,
                Package = p.Package != null ? new PointPackage
                {
                    Id = p.Package.Id,
                    Name = p.Package.Name,
                    Points = p.Package.Points,
                    Price = p.Package.Price
                } : null
            })
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<PaymentHistory?> GetByTransactionIdAsync(string transactionId)
    {
        return await _context.PaymentHistories
            .AsNoTracking()
            .Select(p => new PaymentHistory
            {
                Id = p.Id,
                UserId = p.UserId,
                TransactionId = p.TransactionId,
                OrderCode = p.OrderCode,
                Amount = p.Amount,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                PackageId = p.PackageId,
                User = p.User != null ? new AppUser
                {
                    Id = p.User.Id,
                    Username = p.User.Username,
                    Email = p.User.Email
                } : null,
                Package = p.Package != null ? new PointPackage
                {
                    Id = p.Package.Id,
                    Name = p.Package.Name,
                    Points = p.Package.Points,
                    Price = p.Package.Price
                } : null
            })
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
    }

    public async Task<PaymentHistory?> GetByOrderCodeAsync(string orderCode)
    {
        return await _context.PaymentHistories
            .AsNoTracking()
            .Select(p => new PaymentHistory
            {
                Id = p.Id,
                UserId = p.UserId,
                TransactionId = p.TransactionId,
                OrderCode = p.OrderCode,
                Amount = p.Amount,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                PackageId = p.PackageId,
                User = p.User != null ? new AppUser
                {
                    Id = p.User.Id,
                    Username = p.User.Username,
                    Email = p.User.Email
                } : null,
                Package = p.Package != null ? new PointPackage
                {
                    Id = p.Package.Id,
                    Name = p.Package.Name,
                    Points = p.Package.Points,
                    Price = p.Package.Price
                } : null
            })
            .FirstOrDefaultAsync(p => p.OrderCode == orderCode);
    }

    public async Task<PaymentStatisticsDto> GetStatisticsAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var query = _context.PaymentHistories.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= endDate.Value);
        }

        var now = DateTime.UtcNow;
        var today = now.Date;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

        var statistics = new PaymentStatisticsDto
        {
            TotalPayments = await query.CountAsync(),
            SuccessfulPayments = await query.CountAsync(p => p.Status == "success"),
            PendingPayments = await query.CountAsync(p => p.Status == "pending"),
            FailedPayments = await query.CountAsync(p => p.Status == "failed"),
            TotalRevenue = await query
                .Where(p => p.Status == "success")
                .SumAsync(p => p.Amount),
            TodayRevenue = await query
                .Where(p => p.Status == "success" && p.CreatedAt >= today)
                .SumAsync(p => p.Amount),
            ThisMonthRevenue = await query
                .Where(p => p.Status == "success" && p.CreatedAt >= firstDayOfMonth)
                .SumAsync(p => p.Amount)
        };

        return statistics;
    }

    public async Task<PaymentHistory> CreateAsync(PaymentHistory payment)
    {
        _context.PaymentHistories.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<PaymentHistory> UpdateAsync(PaymentHistory payment)
    {
        _context.PaymentHistories.Update(payment);
        await _context.SaveChangesAsync();
        return payment;
    }
}
