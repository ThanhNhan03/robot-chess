using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

public interface IPointPackageRepository
{
    Task<List<PointPackage>> GetAllAsync();
    Task<List<PointPackage>> GetActivePackagesAsync();
    Task<PointPackage?> GetByIdAsync(int id);
    Task<PointPackage> CreateAsync(PointPackage package);
    Task<PointPackage> UpdateAsync(PointPackage package);
    Task<bool> DeleteAsync(int id);
}
