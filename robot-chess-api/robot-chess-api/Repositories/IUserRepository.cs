using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

public interface IUserRepository
{
    Task<List<AppUser>> GetAllUsersAsync(bool includeInactive = false);
    Task<AppUser?> GetUserByIdAsync(Guid id);
    Task<AppUser?> GetUserByEmailAsync(string email);
    Task<AppUser?> GetUserByUsernameAsync(string username);
    Task<AppUser> CreateUserAsync(AppUser user);
    Task<AppUser> UpdateUserAsync(AppUser user);
    Task<bool> DeleteUserAsync(Guid id); // Soft delete
    Task<bool> UserExistsAsync(Guid id);
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);
    Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null);
    Task<List<AppUser>> GetUsersByRoleAsync(string role);
    Task UpdateLastLoginAsync(Guid userId);
    Task<AppUser?> GetUserByVerificationTokenAsync(string token);
    Task UpdateEmailVerificationAsync(Guid userId, bool verified);
    Task<AppUser?> GetUserByPasswordResetTokenAsync(string token);
}
