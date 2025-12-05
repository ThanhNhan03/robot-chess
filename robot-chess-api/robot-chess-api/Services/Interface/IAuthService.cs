namespace robot_chess_api.Services.Interface;

public interface IAuthService
{
    Task<(bool Success, string? Token, Guid? UserId, string? Error)> SignUpAsync(string email, string password);
    Task<(bool Success, string? Token, Guid? UserId, string? Error)> SignInAsync(string email, string password);
    Task<(bool Success, string? Token, Guid? UserId, string? Error)> LoginAsync(string email, string password);
    Task<(bool Success, string? Error)> LogoutAsync(string token);
    Task<(bool Success, string? Error)> UpdatePasswordAsync(Guid userId, string newPassword);
}
