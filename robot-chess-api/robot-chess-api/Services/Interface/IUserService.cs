using robot_chess_api.DTOs;

namespace robot_chess_api.Services.Interface;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync(bool includeInactive = false);
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task<bool> DeleteUserAsync(Guid id);
    Task<bool> UpdateUserStatusAsync(Guid id, bool isActive);
    Task<List<UserDto>> GetUsersByRoleAsync(string role);
    Task<UserActivityDto?> GetUserActivityAsync(Guid id);
}
