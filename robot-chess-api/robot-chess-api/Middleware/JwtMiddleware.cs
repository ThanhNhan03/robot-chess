using System.Security.Claims;
using System.Text.Json;
using robot_chess_api.Services.Interface;

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

    public async Task InvokeAsync(HttpContext context, Supabase.Client supabaseClient)
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

                    var identity = new ClaimsIdentity(claims, "jwt");
                    context.User = new ClaimsPrincipal(identity);

                    _logger.LogInformation("JWT validated successfully for user: {UserId}", user.Id);
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
