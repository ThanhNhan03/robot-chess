using robot_chest_api.Services.Interface;
using Supabase.Gotrue;

namespace robot_chest_api.Services.Implement;

public class AuthService : IAuthService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly ILogger<AuthService> _logger;

    public AuthService(Supabase.Client supabaseClient, ILogger<AuthService> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    public async Task<(bool Success, string? Token, Guid? UserId, string? Error)> SignUpAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation($"Attempting sign up for: {email}");

            var session = await _supabaseClient.Auth.SignUp(email, password);

            if (session?.User == null)
            {
                _logger.LogWarning($"Sign up failed for: {email}");
                return (false, null, null, "Sign up failed");
            }

            var token = session.AccessToken;
            var userId = Guid.Parse(session.User.Id);

            _logger.LogInformation($"User signed up successfully: {email} (ID: {userId})");
            return (true, token, userId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Sign up error for {email}: {ex.Message}");
            return (false, null, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Token, Guid? UserId, string? Error)> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation($"Attempting login for: {email}");

            var session = await _supabaseClient.Auth.SignIn(email, password);

            if (session?.User == null)
            {
                _logger.LogWarning($"Login failed for: {email}");
                return (false, null, null, "Invalid email or password");
            }

            var token = session.AccessToken;
            var userId = Guid.Parse(session.User.Id);

            _logger.LogInformation($"User logged in successfully: {email} (ID: {userId})");
            return (true, token, userId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login error for {email}: {ex.Message}");
            return (false, null, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> LogoutAsync(string token)
    {
        try
        {
            _logger.LogInformation("Attempting logout");

            await _supabaseClient.Auth.SignOut();

            _logger.LogInformation("User logged out successfully");
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Logout error: {ex.Message}");
            return (false, ex.Message);
        }
    }
}
