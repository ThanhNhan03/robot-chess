using System.Security.Claims;
using System.Text.Json;
using robot_chess_api.Services.Interface;
using robot_chess_api.Repositories;

namespace robot_chess_api.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, Supabase.Client supabaseClient, IUserRepository userRepository)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                // Decode JWT to get user ID without calling Supabase
                var userId = ExtractUserIdFromToken(token);
                
                if (userId != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim("sub", userId), // Standard JWT claim for user ID
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    };

                    // Get user from database to retrieve role and email
                    if (Guid.TryParse(userId, out var userGuid))
                    {
                        var appUser = await userRepository.GetUserByIdAsync(userGuid);
                        if (appUser != null)
                        {
                            claims.Add(new Claim(ClaimTypes.Email, appUser.Email ?? ""));
                            
                            if (!string.IsNullOrEmpty(appUser.Role))
                            {
                                claims.Add(new Claim(ClaimTypes.Role, appUser.Role));
                            }
                            
                            _logger.LogInformation("JWT validated successfully for user: {UserId} with role: {Role}", userId, appUser.Role ?? "no role");
                            
                            var identity = new ClaimsIdentity(claims, "jwt");
                            context.User = new ClaimsPrincipal(identity);
                        }
                        else
                        {
                            _logger.LogWarning("User not found in database: {UserId}", userId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("JWT validation failed: {Message}", ex.Message);
            }
        }

        await _next(context);
    }

    private string? ExtractUserIdFromToken(string token)
    {
        try
        {
            // JWT is base64url encoded: header.payload.signature
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                return null;
            }

            // Decode payload (second part)
            var payload = parts[1];
            
            // Add padding if needed
            var padding = payload.Length % 4;
            if (padding > 0)
            {
                payload += new string('=', 4 - padding);
            }

            // Base64Url decode
            payload = payload.Replace('-', '+').Replace('_', '/');
            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);

            // Parse JSON to get 'sub' claim
            var jsonDoc = JsonDocument.Parse(payloadJson);
            if (jsonDoc.RootElement.TryGetProperty("sub", out var subElement))
            {
                return subElement.GetString();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to extract user ID from token: {Message}", ex.Message);
            return null;
        }
    }
}
