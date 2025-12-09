using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using RobotChessServer.Configuration;
using RobotChessServer.Utilities;

namespace RobotChessServer.Services
{
    /// <summary>
    /// Service quản lý WebSocket connections
    /// </summary>
    public class WebSocketService : IWebSocketService
    {
        private readonly ServerConfig _config;
        private readonly IMessageProcessor _messageProcessor;
        private HttpListener _wsListener;
        private ConcurrentBag<WebSocket> _webSocketClients = new();

        // Delegates
        private Func<object, Task> _broadcastToWebSocket;
        private Func<object, Task> _sendToRobots;
        private Func<object, Task> _sendToAIs;

        public WebSocketService(
            ServerConfig config,
            IMessageProcessor messageProcessor,
            Func<object, Task> broadcastToWebSocket,
            Func<object, Task> sendToRobots,
            Func<object, Task> sendToAIs)
        {
            _config = config;
            _messageProcessor = messageProcessor;
            _broadcastToWebSocket = broadcastToWebSocket;
            _sendToRobots = sendToRobots;
            _sendToAIs = sendToAIs;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                IPAddress ip = await NetworkUtils.GetAvailableIP(_config.PrimaryIP, _config.FallbackIP);
                int port = await NetworkUtils.FindAvailablePort(ip, _config.WebSocketPort, _config.AlternativeWebSocketPorts);

                _wsListener = new HttpListener();

                string prefix = ip.Equals(IPAddress.Any)
                    ? $"http://+:{port}/"
                    : $"http://{ip}:{port}/";

                _wsListener.Prefixes.Add(prefix);
                _wsListener.Start();
                LoggerHelper.LogInfo($"WebSocket server running on {ip}:{port}");

                using var registration = cancellationToken.Register(() => _wsListener.Stop());

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var context = await _wsListener.GetContextAsync();
                        if (context.Request.IsWebSocketRequest)
                        {
                            _ = HandleWebSocketClientAsync(context, cancellationToken);
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                            context.Response.Close();
                        }
                    }
                    catch (HttpListenerException ex) when (ex.ErrorCode == 995 || ex.ErrorCode == 500) // 995 is aborted, often happens on Stop()
                    {
                         if (cancellationToken.IsCancellationRequested) break;
                         LoggerHelper.LogError($"WebSocket listener error: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        LoggerHelper.LogError($"WebSocket accept error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"WebSocket service error: {ex.Message}");
                throw;
            }
        }

        public async Task StopAsync()
        {
            _wsListener?.Stop();
            LoggerHelper.LogInfo("WebSocket server stopped");
        }

        private async Task HandleWebSocketClientAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            WebSocketContext wsContext = null;
            WebSocket ws = null;

            try
            {
                wsContext = await context.AcceptWebSocketAsync(null);
                ws = wsContext.WebSocket;
                _webSocketClients.Add(ws);
                LoggerHelper.LogInfo("WebSocket client connected");

                var buffer = new byte[4096];
                while (ws.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    LoggerHelper.LogDebug($"WebSocket message received: {message}");

                    await _messageProcessor.ProcessWebSocketMessage(
                        message,
                        ws,
                        _sendToRobots,
                        _sendToAIs,
                        SendWebSocketMessageAsync,
                        _broadcastToWebSocket);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"WebSocket error: {ex.Message}");
            }
            finally
            {
                if (ws != null)
                {
                    _webSocketClients = new ConcurrentBag<WebSocket>(_webSocketClients.Where(c => c != ws));
                    ws.Dispose();
                }
                LoggerHelper.LogWarning("WebSocket client disconnected");
            }
        }

        private async Task SendWebSocketMessageAsync(WebSocket ws, object data)
        {
            if (ws.State == WebSocketState.Open)
            {
                var message = JsonSerializer.Serialize(data);
                var buffer = Encoding.UTF8.GetBytes(message);
                await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task BroadcastToWebSocketClientsAsync(object data)
        {
            var message = JsonSerializer.Serialize(data);
            var buffer = Encoding.UTF8.GetBytes(message);

            var tasks = _webSocketClients
                .Where(ws => ws.State == WebSocketState.Open)
                .Select(ws => ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));

            await Task.WhenAll(tasks);
        }

        public ConcurrentBag<WebSocket> GetWebSocketClients() => _webSocketClients;
    }
}
