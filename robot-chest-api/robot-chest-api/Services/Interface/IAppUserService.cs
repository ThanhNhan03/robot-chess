using robot_chest_api.Models;

namespace robot_chest_api.Services.Interface;

public interface IAppUserService
{
    Task<(bool Success, AppUser? User, string? Error)> CreateUserProfileAsync(Guid authUserId, string email, string username);
    Task<AppUser?> GetUserByIdAsync(Guid id);
    Task<AppUser?> GetUserByEmailAsync(string email);
    Task<AppUser?> GetUserByUsernameAsync(string username);
    Task<(bool Success, string? Error)> UpdateUserAsync(Guid id, string? fullName, string? avatarUrl);
    Task<bool> UsernameExistsAsync(string username);
}
