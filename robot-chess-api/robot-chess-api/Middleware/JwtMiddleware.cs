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

        if (token != null)
        {
            try
            {
                await supabaseClient.Auth.SetSession(token, token);
                var user = supabaseClient.Auth.CurrentUser;
                
                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Email, user.Email ?? "")
                    };

                    // Get user from database to retrieve role
                    if (Guid.TryParse(user.Id, out var userId))
                    {
                        var appUser = await userRepository.GetUserByIdAsync(userId);
                        if (appUser != null && !string.IsNullOrEmpty(appUser.Role))
                        {
                            claims.Add(new Claim(ClaimTypes.Role, appUser.Role));
                            _logger.LogInformation("JWT validated successfully for user: {UserId} with role: {Role}", user.Id, appUser.Role);
                        }
                        else
                        {
                            _logger.LogInformation("JWT validated successfully for user: {UserId} (no role)", user.Id);
                        }
                    }

                    var identity = new ClaimsIdentity(claims, "jwt");
                    context.User = new ClaimsPrincipal(identity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("JWT validation failed: {Message}", ex.Message);
            }
        }

        await _next(context);
    }
}
