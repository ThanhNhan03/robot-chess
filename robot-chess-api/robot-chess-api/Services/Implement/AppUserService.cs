using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.Models;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Services.Implement;

public class AppUserService : IAppUserService
{
    private readonly PostgresContext _context;
    private readonly ILogger<AppUserService> _logger;

    public AppUserService(PostgresContext context, ILogger<AppUserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, AppUser? User, string? Error)> CreateUserProfileAsync(
        Guid authUserId, 
        string email, 
        string username)
    {
        try
        {
            _logger.LogInformation($"Creating user profile for: {username}");

            // Check if username already exists
            var existingUser = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == username);

            if (existingUser != null)
            {
                _logger.LogWarning($"Username already exists: {username}");
                return (false, null, "Username already exists");
            }

            // Check if email already exists
            var existingEmail = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingEmail != null)
            {
                _logger.LogWarning($"Email already exists: {email}");
                return (false, null, "Email already exists");
            }

            var appUser = new AppUser
            {
                Id = authUserId,
                Email = email,
                Username = username,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.AppUsers.Add(appUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User profile created: {username} (ID: {authUserId})");
            return (true, appUser, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"CreateUserProfile error: {ex.Message}");
            _logger.LogError($"Inner exception: {ex.InnerException?.Message}");
            _logger.LogError($"Stack trace: {ex.StackTrace}");
            return (false, null, ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<AppUser?> GetUserByIdAsync(Guid id)
    {
        try
        {
            return await _context.AppUsers
                .Include(u => u.Games)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetUserById error: {ex.Message}");
            return null;
        }
    }

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        try
        {
            return await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetUserByEmail error: {ex.Message}");
            return null;
        }
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        try
        {
            return await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == username);
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetUserByUsername error: {ex.Message}");
            return null;
        }
    }

    public async Task<(bool Success, string? Error)> UpdateUserAsync(Guid id, string? fullName, string? avatarUrl)
    {
        try
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User not found: {id}");
                return (false, "User not found");
            }

            if (fullName != null) user.FullName = fullName;
            if (avatarUrl != null) user.AvatarUrl = avatarUrl;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"User updated: {user.Username}");
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"UpdateUser error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        try
        {
            return await _context.AppUsers.AnyAsync(u => u.Username == username);
        }
        catch (Exception ex)
        {
            _logger.LogError($"UsernameExists error: {ex.Message}");
            return false;
        }
    }
}
