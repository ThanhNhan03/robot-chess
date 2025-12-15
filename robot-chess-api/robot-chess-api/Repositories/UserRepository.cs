using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PostgresContext _context;

    public UserRepository(PostgresContext context)
    {
        _context = context;
    }

    public async Task<List<AppUser>> GetAllUsersAsync(bool includeInactive = false)
    {
        var query = _context.AppUsers.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(u => u.IsActive);
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<AppUser?> GetUserByIdAsync(Guid id)
    {
        return await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        return await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        return await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<AppUser> CreateUserAsync(AppUser user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.IsActive = true;

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<AppUser> UpdateUserAsync(AppUser user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.AppUsers.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await GetUserByIdAsync(id);
        if (user == null) return false;

        // Soft delete
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UserExistsAsync(Guid id)
    {
        return await _context.AppUsers.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
    {
        var query = _context.AppUsers.Where(u => u.Email == email);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null)
    {
        var query = _context.AppUsers.Where(u => u.Username == username);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<AppUser>> GetUsersByRoleAsync(string role)
    {
        return await _context.AppUsers
            .Where(u => u.Role == role && u.IsActive)
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<AppUser?> GetUserByVerificationTokenAsync(string token)
    {
        return await _context.AppUsers
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
    }

    public async Task UpdateEmailVerificationAsync(Guid userId, bool verified)
    {
        var user = await GetUserByIdAsync(userId);
        if (user != null)
        {
            user.EmailVerified = verified;
            if (verified)
            {
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiry = null;
            }
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<AppUser?> GetUserByPasswordResetTokenAsync(string token)
    {
        return await _context.AppUsers
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token);
    }

    public async Task<List<AppUser>> GetUsersByIdsAsync(List<Guid> userIds)
    {
        return await _context.AppUsers
            .Where(u => userIds.Contains(u.Id) && u.IsActive)
            .ToListAsync();
    }

    public async Task<List<AppUser>> GetUsersByEmailsAsync(List<string> emails)
    {
        return await _context.AppUsers
            .Where(u => emails.Contains(u.Email) && u.IsActive && u.EmailVerified)
            .ToListAsync();
    }

    public async Task<List<AppUser>> GetAllPlayersAsync()
    {
        return await _context.AppUsers
            .Where(u => u.Role == "player" && u.IsActive && u.EmailVerified)
            .ToListAsync();
    }
}
