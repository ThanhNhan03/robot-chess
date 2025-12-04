using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.DTOs;
using robot_chess_api.Models;
using robot_chess_api.Repositories;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Services.Implement;

public class PointPackageService : IPointPackageService
{
    private readonly IPointPackageRepository _packageRepository;
    private readonly IUserRepository _userRepository;
    private readonly PostgresContext _context;
    private readonly ILogger<PointPackageService> _logger;

    public PointPackageService(
        IPointPackageRepository packageRepository,
        IUserRepository userRepository,
        PostgresContext context,
        ILogger<PointPackageService> logger)
    {
        _packageRepository = packageRepository;
        _userRepository = userRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<List<PointPackageDto>> GetAllPackagesAsync()
    {
        var packages = await _packageRepository.GetAllAsync();
        return packages.Select(MapToDto).ToList();
    }

    public async Task<List<PointPackageDto>> GetActivePackagesAsync()
    {
        var packages = await _packageRepository.GetActivePackagesAsync();
        return packages.Select(MapToDto).ToList();
    }

    public async Task<PointPackageDto?> GetPackageByIdAsync(int id)
    {
        var package = await _packageRepository.GetByIdAsync(id);
        return package == null ? null : MapToDto(package);
    }

    public async Task<PointPackageDto> CreatePackageAsync(CreatePointPackageDto dto)
    {
        var package = new PointPackage
        {
            Name = dto.Name,
            Points = dto.Points,
            Price = dto.Price,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _packageRepository.CreateAsync(package);
        _logger.LogInformation($"Point package created: {created.Name} ({created.Points} points for ${created.Price})");
        
        return MapToDto(created);
    }

    public async Task<PointPackageDto> UpdatePackageAsync(int id, UpdatePointPackageDto dto)
    {
        var package = await _packageRepository.GetByIdAsync(id);
        if (package == null)
        {
            throw new KeyNotFoundException($"Point package with ID {id} not found");
        }

        if (dto.Name != null) package.Name = dto.Name;
        if (dto.Points.HasValue) package.Points = dto.Points.Value;
        if (dto.Price.HasValue) package.Price = dto.Price.Value;
        if (dto.Description != null) package.Description = dto.Description;
        if (dto.IsActive.HasValue) package.IsActive = dto.IsActive.Value;

        var updated = await _packageRepository.UpdateAsync(package);
        _logger.LogInformation($"Point package updated: {updated.Name}");
        
        return MapToDto(updated);
    }

    public async Task<bool> DeletePackageAsync(int id)
    {
        var result = await _packageRepository.DeleteAsync(id);
        if (result)
        {
            _logger.LogInformation($"Point package deleted: ID {id}");
        }
        return result;
    }

    public async Task<(bool Success, string Message)> PurchasePackageAsync(Guid userId, int packageId, string transactionId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Get package
            var package = await _packageRepository.GetByIdAsync(packageId);
            if (package == null)
            {
                return (false, "Point package not found");
            }

            if (!package.IsActive)
            {
                return (false, "Point package is not active");
            }

            // Get user
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            // Create payment history
            var payment = new PaymentHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TransactionId = transactionId,
                Amount = package.Price,
                Status = "success",
                CreatedAt = DateTime.UtcNow
            };
            _context.PaymentHistories.Add(payment);

            // Create point transaction
            var pointTransaction = new PointTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = package.Points,
                TransactionType = "deposit",
                Description = $"Purchased {package.Name} package",
                RelatedPaymentId = payment.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.PointTransactions.Add(pointTransaction);

            // Update user points balance
            user.PointsBalance += package.Points;
            _context.AppUsers.Update(user);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation($"User {userId} purchased {package.Name} package. New balance: {user.PointsBalance} points");
            
            return (true, $"Successfully purchased {package.Name}. You now have {user.PointsBalance} points.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, $"Error purchasing package for user {userId}");
            return (false, "An error occurred while processing your purchase");
        }
    }

    public async Task<(bool Success, string Message)> UsePointsAsync(Guid userId, int amount, string description)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Get user
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            // Check if user has enough points
            if (user.PointsBalance < amount)
            {
                return (false, $"Insufficient points. You have {user.PointsBalance} points but need {amount} points.");
            }

            // Create point transaction
            var pointTransaction = new PointTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = -amount, // Negative for usage
                TransactionType = "service_usage",
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _context.PointTransactions.Add(pointTransaction);

            // Update user points balance
            user.PointsBalance -= amount;
            _context.AppUsers.Update(user);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation($"User {userId} used {amount} points for: {description}. Remaining balance: {user.PointsBalance} points");
            
            return (true, $"Successfully used {amount} points. Remaining balance: {user.PointsBalance} points.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, $"Error using points for user {userId}");
            return (false, "An error occurred while using points");
        }
    }

    public async Task<List<PointTransactionDto>> GetUserTransactionsAsync(Guid userId)
    {
        var transactions = await _context.PointTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(50) // Limit to last 50 transactions
            .ToListAsync();

        return transactions.Select(t => new PointTransactionDto
        {
            Id = t.Id,
            UserId = t.UserId,
            Amount = t.Amount,
            TransactionType = t.TransactionType,
            Description = t.Description,
            RelatedPaymentId = t.RelatedPaymentId,
            CreatedAt = t.CreatedAt
        }).ToList();
    }

    private PointPackageDto MapToDto(PointPackage package)
    {
        return new PointPackageDto
        {
            Id = package.Id,
            Name = package.Name,
            Points = package.Points,
            Price = package.Price,
            Description = package.Description,
            IsActive = package.IsActive,
            CreatedAt = package.CreatedAt
        };
    }
}
