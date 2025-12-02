using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using RobotChessServer.Configuration;
using RobotChessServer.Models;
using RobotChessServer.Services;
using RobotChessServer.Utilities;

namespace RobotChessServer
{
    class Program
    {
        private static ServerConfig _config;
        private static ITcpService _tcpService;
        private static IWebSocketService _wsService;
        private static IHttpCommandService _httpCommandService;
        private static IApiService _apiService;
        private static IMessageProcessor _messageProcessor;
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        // Client storage
        private static ConcurrentBag<ClientInfo> _robotClients = new();
        private static ConcurrentBag<ClientInfo> _aiClients = new();

        static async Task Main(string[] args)
        {
            try
            {
                // Load configuration - use fully qualified name to avoid ambiguity
                _config = RobotChessServer.Configuration.ConfigurationManager.LoadConfiguration();
                _config.PrintConfiguration();

                // Initialize services
                InitializeServices();

                // Handle Ctrl+C
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    LoggerHelper.LogWarning("Shutting down server...");
                    _cts.Cancel();
                };

                // Start servers
                var tcpTask = _tcpService.StartAsync(_cts.Token);
                var wsTask = _wsService.StartAsync(_cts.Token);
                var httpTask = _httpCommandService.StartAsync(_cts.Token);

                // Status logging
                var statusTask = StatusLoggingAsync();

                await Task.WhenAll(tcpTask, wsTask, httpTask, statusTask);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"Critical error: {ex.Message}");
            }
            finally
            {
                await ShutdownAsync();
            }
        }

        private static void InitializeServices()
        {
            // Initialize API Service
            _apiService = new ApiService("https://localhost:7096");

            // Initialize message processor
            _messageProcessor = new MessageProcessor();

            // Initialize TCP Service
            _tcpService = new TcpService(
                _config,
                _messageProcessor,
                BroadcastToWebSocketAsync,
                SendCommandToRobotsAsync,
                SendRequestToAIsAsync
            );

            // Initialize WebSocket Service
            _wsService = new WebSocketService(
                _config,
                _messageProcessor,
                BroadcastToWebSocketAsync,
                SendCommandToRobotsAsync,
                SendRequestToAIsAsync
            );

            // Initialize HTTP Command Service (receives commands from REST API)
            _httpCommandService = new HttpCommandService(5000, SendCommandToRobotsAsync, SendRequestToAIsAsync);
        }

        private static async Task BroadcastToWebSocketAsync(object data)
        {
            if (_wsService is WebSocketService wsService)
            {
                await wsService.BroadcastToWebSocketClientsAsync(data);
            }
        }

        private static async Task SendCommandToRobotsAsync(object command)
        {
            LoggerHelper.LogInfo($"Sending command to robot clients");
            var message = JsonSerializer.Serialize(command) + "\n";
            var buffer = Encoding.UTF8.GetBytes(message);

            if (_tcpService is TcpService tcpService)
            {
                var robotClients = tcpService.GetRobotClients();
                foreach (var robot in robotClients)
                {
                    try
                    {
                        var stream = robot.Client.GetStream();
                        await stream.WriteAsync(buffer, 0, buffer.Length);
                        LoggerHelper.LogDebug($"Sent command to robot {robot.RobotId}");
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.LogError($"Error sending to robot: {ex.Message}");
                    }
                }
            }
        }

        private static async Task SendRequestToAIsAsync(object request)
        {
            LoggerHelper.LogInfo($"Sending request to AI clients");
            var message = JsonSerializer.Serialize(request) + "\n";
            var buffer = Encoding.UTF8.GetBytes(message);

            if (_tcpService is TcpService tcpService)
            {
                var aiClients = tcpService.GetAIClients();
                foreach (var ai in aiClients)
                {
                    try
                    {
                        var stream = ai.Client.GetStream();
                        await stream.WriteAsync(buffer, 0, buffer.Length);
                        LoggerHelper.LogDebug($"Sent request to AI {ai.AIId}");
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.LogError($"Error sending to AI: {ex.Message}");
                    }
                }
            }
        }

        private static async Task StatusLoggingAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(30000, _cts.Token);

                    if (_tcpService is TcpService tcpService && _wsService is WebSocketService wsService)
                    {
                        var robotCount = tcpService.GetRobotClients().Count;
                        var aiCount = tcpService.GetAIClients().Count;
                        var wsCount = wsService.GetWebSocketClients().Count;

                        LoggerHelper.LogInfo($"Status: {robotCount} Robots, {aiCount} AIs, {wsCount} WebSocket clients");
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    LoggerHelper.LogError($"Status logging error: {ex.Message}");
                }
            }
        }

        private static async Task ShutdownAsync()
        {
            LoggerHelper.LogWarning("Server shutting down...");
            await _tcpService.StopAsync();
            await _wsService.StopAsync();
            await _httpCommandService.StopAsync();
            LoggerHelper.LogInfo("Server stopped completely");
        }
    }
}