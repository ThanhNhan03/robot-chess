using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

public class PointPackageRepository : IPointPackageRepository
{
    private readonly PostgresContext _context;

    public PointPackageRepository(PostgresContext context)
    {
        _context = context;
    }

    public async Task<List<PointPackage>> GetAllAsync()
    {
        return await _context.PointPackages
            .OrderBy(p => p.Points)
            .ToListAsync();
    }

    public async Task<List<PointPackage>> GetActivePackagesAsync()
    {
        return await _context.PointPackages
            .Where(p => p.IsActive)
            .OrderBy(p => p.Points)
            .ToListAsync();
    }

    public async Task<PointPackage?> GetByIdAsync(int id)
    {
        return await _context.PointPackages.FindAsync(id);
    }

    public async Task<PointPackage> CreateAsync(PointPackage package)
    {
        _context.PointPackages.Add(package);
        await _context.SaveChangesAsync();
        return package;
    }

    public async Task<PointPackage> UpdateAsync(PointPackage package)
    {
        package.UpdatedAt = DateTime.UtcNow;
        _context.PointPackages.Update(package);
        await _context.SaveChangesAsync();
        return package;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var package = await GetByIdAsync(id);
        if (package == null)
            return false;

        _context.PointPackages.Remove(package);
        await _context.SaveChangesAsync();
        return true;
    }
}
