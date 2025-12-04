using robot_chess_api.DTOs;

namespace robot_chess_api.Services.Interface;

public interface IPointPackageService
{
    Task<List<PointPackageDto>> GetAllPackagesAsync();
    Task<List<PointPackageDto>> GetActivePackagesAsync();
    Task<PointPackageDto?> GetPackageByIdAsync(int id);
    Task<PointPackageDto> CreatePackageAsync(CreatePointPackageDto dto);
    Task<PointPackageDto> UpdatePackageAsync(int id, UpdatePointPackageDto dto);
    Task<bool> DeletePackageAsync(int id);
    Task<(bool Success, string Message)> PurchasePackageAsync(Guid userId, int packageId, string transactionId);
    Task<(bool Success, string Message)> UsePointsAsync(Guid userId, int amount, string description);
    Task<List<PointTransactionDto>> GetUserTransactionsAsync(Guid userId);
}
