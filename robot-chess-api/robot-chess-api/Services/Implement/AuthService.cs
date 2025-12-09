using robot_chess_api.Services.Interface;
using robot_chess_api.Models;
using Supabase.Gotrue;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace robot_chess_api.Services.Implement;

public class AuthService : IAuthService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(Supabase.Client supabaseClient, ILogger<AuthService> logger, IConfiguration configuration)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<(bool Success, string? Token, Guid? UserId, string? Error)> SignUpAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation($"Attempting sign up for: {email}");

            var session = await _supabaseClient.Auth.SignUp(email, password);

            if (session?.User == null)
            {
                _logger.LogWarning($"Sign up failed for: {email} - No user returned");
                return (false, null, null, "Đăng ký thất bại");
            }

            var token = session.AccessToken;
            var userId = Guid.Parse(session.User.Id);

            _logger.LogInformation($"User signed up successfully: {email} (ID: {userId})");
            return (true, token, userId, null);
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            _logger.LogError($"Supabase Auth error during signup for {email}: {ex.Message}");
            
            if (ex.Message.Contains("User already registered"))
            {
                return (false, null, null, "Email đã được đăng ký");
            }
            
            return (false, null, null, "Lỗi đăng ký: " + ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Network error during signup for {email}: {ex.Message}");
            return (false, null, null, "Lỗi kết nối đến server xác thực. Vui lòng thử lại sau.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Sign up error for {email}: {ex.Message}");
            _logger.LogError($"Stack trace: {ex.StackTrace}");
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
                _logger.LogWarning($"Login failed for: {email} - No user returned");
                return (false, null, null, "Invalid email or password");
            }

            var token = session.AccessToken;
            var userId = Guid.Parse(session.User.Id);

            _logger.LogInformation($"User logged in successfully: {email} (ID: {userId})");
            return (true, token, userId, null);
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            _logger.LogError($"Supabase Auth error for {email}: {ex.Message}");
            
            // Check for specific errors
            if (ex.Message.Contains("Email not confirmed"))
            {
                return (false, null, null, "Email chưa được xác thực. Vui lòng kiểm tra email của bạn.");
            }
            if (ex.Message.Contains("Invalid login credentials"))
            {
                return (false, null, null, "Email hoặc mật khẩu không đúng");
            }
            
            return (false, null, null, "Lỗi xác thực: " + ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Network error during login for {email}: {ex.Message}");
            return (false, null, null, "Lỗi kết nối đến server xác thực. Vui lòng thử lại sau.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login error for {email}: {ex.Message}");
            _logger.LogError($"Stack trace: {ex.StackTrace}");
            return (false, null, null, ex.Message);
        }
    }

    // Alias for LoginAsync to support both naming conventions
    public async Task<(bool Success, string? Token, Guid? UserId, string? Error)> SignInAsync(string email, string password)
    {
        return await LoginAsync(email, password);
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

    public async Task<(bool Success, string? Error)> UpdatePasswordAsync(Guid userId, string newPassword)
    {
        try
        {
            _logger.LogInformation($"Updating password for user: {userId}");

            // Get service role key from configuration
            var supabaseUrl = _configuration["Supabase:Url"] 
                ?? throw new InvalidOperationException("Supabase:Url not configured");
            var serviceRoleKey = _configuration["ServiceRoleKey"] 
                ?? throw new InvalidOperationException("ServiceRoleKey not configured");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", serviceRoleKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {serviceRoleKey}");

            var updateData = new
            {
                password = newPassword
            };

            var response = await httpClient.PutAsJsonAsync(
                $"{supabaseUrl}/auth/v1/admin/users/{userId}",
                updateData
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Password update failed: {error}");
                return (false, "Failed to update password");
            }

            _logger.LogInformation($"Password updated successfully for user: {userId}");
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"UpdatePassword error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, Guid? UserId, string? Email, string? Error)> VerifyGoogleTokenAsync(string accessToken)
    {
        try
        {
            _logger.LogInformation("Verifying Google access token");

            var supabaseUrl = _configuration["Supabase:Url"];
            var anonKey = _configuration["Supabase:AnonKey"];

            if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(anonKey))
            {
                return (false, null, null, "Supabase configuration missing");
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", anonKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await client.GetAsync($"{supabaseUrl}/auth/v1/user");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Token verification failed: {errorContent}");
                return (false, null, null, "Invalid access token");
            }

            var userData = await response.Content.ReadFromJsonAsync<SupabaseUserResponse>();

            if (userData?.Id == null)
            {
                return (false, null, null, "User data not found");
            }

            _logger.LogInformation($"Token verified successfully for user: {userData.Email}");
            return (true, Guid.Parse(userData.Id), userData.Email, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Google token verification failed: {ex.Message}");
            return (false, null, null, ex.Message);
        }
    }

    public async Task<string> GenerateJwtTokenAsync(robot_chess_api.Models.AppUser user)
    {
        try
        {
            // For now, we'll use the Supabase token
            // In a production scenario, you might want to generate your own JWT
            _logger.LogInformation($"Generating JWT token for user: {user.Email}");

            // Create a session with Supabase to get a token
            // Since the user is already authenticated via Google, we can use their ID
            var jwtSecret = _configuration["Jwt:Secret"] ?? "your-secret-key-change-this-in-production";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "robot-chess-api";
            
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(jwtSecret);
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("sub", user.Id.ToString()),
                    new System.Security.Claims.Claim("email", user.Email),
                    new System.Security.Claims.Claim("role", user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = jwtIssuer,
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError($"JWT generation error: {ex.Message}");
            throw;
        }
    }
}

// Helper class for deserializing Supabase user response
public class SupabaseUserResponse
{
    public string? Id { get; set; }
    public string? Email { get; set; }
}
