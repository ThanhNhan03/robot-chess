using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using RobotChessServer.Configuration;
using RobotChessServer.Models;
using RobotChessServer.Utilities;

namespace RobotChessServer.Services
{
    /// <summary>
    /// Service quản lý TCP connections
    /// </summary>
    public class TcpService : ITcpService
    {
        private readonly ServerConfig _config;
        private readonly IMessageProcessor _messageProcessor;
        private TcpListener _tcpListener;
        private ConcurrentBag<ClientInfo> _robotClients = new();
        private ConcurrentBag<ClientInfo> _aiClients = new();

        // Delegates cho broadcast và send
        private Func<object, Task> _broadcastToWebSocket;
        private Func<object, Task> _sendToRobots;
        private Func<object, Task> _sendToAIs;

        public TcpService(
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
                int port = await NetworkUtils.FindAvailablePort(ip, _config.TcpPort, _config.AlternativeTcpPorts);

                _tcpListener = new TcpListener(ip, port);
                _tcpListener.Start();
                LoggerHelper.LogInfo($"TCP server running on {ip}:{port}");

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var client = await _tcpListener.AcceptTcpClientAsync();
                        _ = HandleClientAsync(client, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            LoggerHelper.LogError($"TCP accept error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"TCP Service error: {ex.Message}");
                throw;
            }
        }

        public async Task StopAsync()
        {
            _tcpListener?.Stop();
            LoggerHelper.LogInfo("TCP server stopped");
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            var clientInfo = new ClientInfo { Client = client };
            LoggerHelper.LogInfo($"TCP client connected from: {client.Client.RemoteEndPoint}");

            try
            {
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    while (!cancellationToken.IsCancellationRequested && client.Connected)
                    {
                        var message = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(message)) break;

                        LoggerHelper.LogDebug($"Received data from TCP client: {message}");
                        await _messageProcessor.ProcessTcpMessage(
                            message,
                            clientInfo,
                            writer,
                            _robotClients,
                            _aiClients,
                            _broadcastToWebSocket,
                            _sendToRobots,
                            _sendToAIs);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"TCP client handling error: {ex.Message}");
            }
            finally
            {
                if (clientInfo.IsRobot)
                {
                    _robotClients = new ConcurrentBag<ClientInfo>(_robotClients.Where(c => c != clientInfo));
                    LoggerHelper.LogWarning($"Robot {clientInfo.RobotId} disconnected");
                }
                if (clientInfo.IsAI)
                {
                    _aiClients = new ConcurrentBag<ClientInfo>(_aiClients.Where(c => c != clientInfo));
                    LoggerHelper.LogWarning($"AI {clientInfo.AIId} disconnected");
                }
                client.Close();
            }
        }

        public ConcurrentBag<ClientInfo> GetRobotClients() => _robotClients;
        public ConcurrentBag<ClientInfo> GetAIClients() => _aiClients;
    }
}
