using System.Collections.Concurrent;
using System.Net.WebSockets;
using RobotChessServer.Models;

namespace RobotChessServer.Services
{
    /// <summary>
    /// Interface xử lý message
    /// </summary>
    public interface IMessageProcessor
    {
        Task ProcessTcpMessage(string message, ClientInfo clientInfo, StreamWriter writer, ConcurrentBag<ClientInfo> robotClients, ConcurrentBag<ClientInfo> aiClients, Func<object, Task> broadcastWs, Func<object, Task> sendToRobots, Func<object, Task> sendToAIs);
        Task ProcessWebSocketMessage(string message, WebSocket ws, Func<object, Task> sendToRobots, Func<object, Task> sendToAIs, Func<WebSocket, object, Task> sendWsMessage, Func<object, Task> broadcastWs);
    }
}
