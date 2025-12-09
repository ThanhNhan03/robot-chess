using Microsoft.AspNetCore.SignalR;

namespace robot_chess_api.Hubs;

public class AuthHub : Hub
{
    private static readonly Dictionary<string, string> _qrSessions = new();
    private readonly ILogger<AuthHub> _logger;

    public AuthHub(ILogger<AuthHub> logger)
    {
        _logger = logger;
    }

    public async Task RegisterQRSession(string sessionId)
    {
        _qrSessions[sessionId] = Context.ConnectionId;
        _logger.LogInformation($"QR Session registered: {sessionId} -> {Context.ConnectionId}");
        await Clients.Caller.SendAsync("SessionRegistered", sessionId);
    }

    public async Task NotifyLoginSuccess(string sessionId, string token, object user)
    {
        if (_qrSessions.TryGetValue(sessionId, out var connectionId))
        {
            _logger.LogInformation($"Notifying login success for session: {sessionId}");
            await Clients.Client(connectionId).SendAsync("LoginSuccess", token, user);
            _qrSessions.Remove(sessionId);
        }
        else
        {
            _logger.LogWarning($"Session not found: {sessionId}");
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up sessions when client disconnects
        var session = _qrSessions.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (session.Key != null)
        {
            _qrSessions.Remove(session.Key);
            _logger.LogInformation($"Session removed on disconnect: {session.Key}");
        }
        return base.OnDisconnectedAsync(exception);
    }
}
