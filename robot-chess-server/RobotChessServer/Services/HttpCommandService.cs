using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text.Json;
using RobotChessServer.Utilities;

namespace RobotChessServer.Services
{
    /// <summary>
    /// HTTP Server to receive commands from REST API
    /// </summary>
    public class HttpCommandService : IHttpCommandService
    {
        private WebApplication? _app;
        private readonly int _port;
        private readonly Func<object, Task> _sendToRobots;
        private readonly Func<object, Task> _sendToAIs;

        public HttpCommandService(int port, Func<object, Task> sendToRobots, Func<object, Task> sendToAIs)
        {
            _port = port;
            _sendToRobots = sendToRobots;
            _sendToAIs = sendToAIs;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls($"http://localhost:{_port}");
            
            _app = builder.Build();

            // Endpoint to receive commands from REST API
            _app.MapPost("/internal/command", async (HttpContext context) =>
            {
                try
                {
                    // Read raw body for debugging
                    using var reader = new StreamReader(context.Request.Body);
                    var rawBody = await reader.ReadToEndAsync();
                    LoggerHelper.LogDebug($"Raw request body: {rawBody}");

                    // Deserialize with case-insensitive options
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var command = JsonSerializer.Deserialize<CommandRequest>(rawBody, options);
                    
                    if (command == null)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsJsonAsync(new { error = "Invalid command" });
                        return;
                    }

                    LoggerHelper.LogInfo($"Received command from API: {command.CommandType} for robot {command.RobotId}");
                    LoggerHelper.LogDebug($"Payload ValueKind: {command.Payload.ValueKind}");

                    // Forward command to robot via TCP
                    await _sendToRobots(command);

                    await context.Response.WriteAsJsonAsync(new 
                    { 
                        success = true, 
                        message = "Command sent to robot",
                        commandId = command.CommandId
                    });
                }
                catch (Exception ex)
                {
                    LoggerHelper.LogError($"Error processing command: {ex.Message}");
                    LoggerHelper.LogError($"Stack trace: {ex.StackTrace}");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { error = ex.Message });
                }
            });

            // Endpoint to receive AI commands from REST API (for game start/resume)
            _app.MapPost("/internal/ai-command", async (HttpContext context) =>
            {
                try
                {
                    using var reader = new StreamReader(context.Request.Body);
                    var rawBody = await reader.ReadToEndAsync();
                    LoggerHelper.LogDebug($"AI Command raw body: {rawBody}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var aiRequest = JsonSerializer.Deserialize<AICommandRequest>(rawBody, options);
                    
                    if (aiRequest == null)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsJsonAsync(new { error = "Invalid AI request" });
                        return;
                    }

                    LoggerHelper.LogInfo($"Received AI command: {aiRequest.Command} (Request ID: {aiRequest.RequestId})");

                    // Forward to AI clients via TCP
                    await _sendToAIs(aiRequest);

                    await context.Response.WriteAsJsonAsync(new 
                    { 
                        success = true, 
                        message = "Command sent to AI",
                        requestId = aiRequest.RequestId
                    });
                }
                catch (Exception ex)
                {
                    LoggerHelper.LogError($"Error processing AI command: {ex.Message}");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { error = ex.Message });
                }
            });

            // Health check endpoint
            _app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

            LoggerHelper.LogInfo($"HTTP Command Server starting on port {_port}");
            await _app.StartAsync(cancellationToken);
            LoggerHelper.LogInfo($"HTTP Command Server running on http://localhost:{_port}");
        }

        public async Task StopAsync()
        {
            if (_app != null)
            {
                await _app.StopAsync();
                LoggerHelper.LogInfo("HTTP Command Server stopped");
            }
        }
    }

    public class CommandRequest
    {
        public Guid CommandId { get; set; }
        public string RobotId { get; set; } = string.Empty;
        public string CommandType { get; set; } = string.Empty;
        public JsonElement Payload { get; set; }
    }

    public class AICommandRequest
    {
        public string Type { get; set; } = "ai_request";
        public string RequestId { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public JsonElement Payload { get; set; }
    }
}
