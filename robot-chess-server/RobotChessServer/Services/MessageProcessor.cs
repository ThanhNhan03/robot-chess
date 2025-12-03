using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using RobotChessServer.Models;
using RobotChessServer.Utilities;

namespace RobotChessServer.Services
{
    /// <summary>
    /// Message processor for TCP and WebSocket
    /// </summary>
    public class MessageProcessor : IMessageProcessor
    {
        public async Task ProcessTcpMessage(
            string message,
            ClientInfo clientInfo,
            StreamWriter writer,
            ConcurrentBag<ClientInfo> robotClients,
            ConcurrentBag<ClientInfo> aiClients,
            Func<object, Task> broadcastWs,
            Func<object, Task> sendToRobots,
            Func<object, Task> sendToAIs)
        {
            try
            {
                var doc = JsonDocument.Parse(message);
                var root = doc.RootElement;

                // Robot identify
                if (root.TryGetProperty("type", out var type) && type.GetString() == "robot_identify")
                {
                    var robotId = root.GetProperty("robot_id").GetString();
                    clientInfo.IsRobot = true;
                    clientInfo.RobotId = robotId;
                    robotClients.Add(clientInfo);
                    LoggerHelper.LogInfo($"Robot client identified: {robotId}");

                    await writer.WriteLineAsync(JsonSerializer.Serialize(new
                    {
                        status = "robot_registered",
                        message = "Robot registered successfully",
                        robot_id = robotId,
                        timestamp = DateTime.UtcNow
                    }));
                    return;
                }

                // AI identify
                if (root.TryGetProperty("type", out type) && type.GetString() == "ai_identify")
                {
                    var aiId = root.GetProperty("ai_id").GetString();
                    clientInfo.IsAI = true;
                    clientInfo.AIId = aiId;
                    aiClients.Add(clientInfo);
                    LoggerHelper.LogInfo($"AI client identified: {aiId}");

                    await writer.WriteLineAsync(JsonSerializer.Serialize(new
                    {
                        status = "ai_registered",
                        message = "AI registered successfully",
                        ai_id = aiId,
                        timestamp = DateTime.UtcNow
                    }));
                    return;
                }

                // Board status from AI (New)
                if (root.TryGetProperty("type", out type) && type.GetString() == "board_status")
                {
                    var status = root.GetProperty("status").GetString();
                    LoggerHelper.LogInfo($"Received board status: {status}");

                    // Broadcast to WebSocket clients
                    await broadcastWs(new
                    {
                        type = "board_status",
                        status = status,
                        game_id = root.TryGetProperty("game_id", out var gid) ? gid.GetString() : null,
                        fen = root.TryGetProperty("fen", out var f) ? f.GetString() : null,
                        expected = root.TryGetProperty("expected", out var e) ? e.GetString() : null,
                        detected = root.TryGetProperty("detected", out var d) ? d.GetString() : null,
                        message = root.TryGetProperty("message", out var m) ? m.GetString() : null,
                        timestamp = DateTime.UtcNow
                    });
                    return;
                }

                // Check detected from AI
                if (root.TryGetProperty("type", out type) && type.GetString() == "check_detected")
                {
                    var playerInCheck = root.TryGetProperty("player_in_check", out var pic) ? pic.GetString() : "unknown";
                    LoggerHelper.LogInfo($"Check detected - Player in check: {playerInCheck}");

                    // Broadcast to WebSocket clients
                    await broadcastWs(new
                    {
                        type = "check_detected",
                        game_id = root.TryGetProperty("game_id", out var gid) ? gid.GetString() : null,
                        player_in_check = playerInCheck,
                        fen_str = root.TryGetProperty("fen_str", out var checkFen) ? checkFen.GetString() : null,
                        message = root.TryGetProperty("message", out var msg) ? msg.GetString() : null,
                        timestamp = DateTime.UtcNow
                    });
                    return;
                }

                // Game over from AI (checkmate/stalemate)
                if (root.TryGetProperty("type", out type) && type.GetString() == "game_over")
                {
                    var reason = root.TryGetProperty("reason", out var r) ? r.GetString() : "unknown";
                    var winner = root.TryGetProperty("winner", out var w) ? w.GetString() : null;
                    LoggerHelper.LogInfo($"Game over - Reason: {reason}, Winner: {winner ?? "draw"}");

                    // Broadcast to WebSocket clients
                    await broadcastWs(new
                    {
                        type = "game_over",
                        game_id = root.TryGetProperty("game_id", out var gid) ? gid.GetString() : null,
                        reason = reason,
                        winner = winner,
                        fen_str = root.TryGetProperty("fen_str", out var gameOverFen) ? gameOverFen.GetString() : null,
                        message = root.TryGetProperty("message", out var msg) ? msg.GetString() : null,
                        timestamp = DateTime.UtcNow
                    });
                    return;
                }

                // Illegal move from AI
                if (root.TryGetProperty("type", out type) && type.GetString() == "illegal_move")
                {
                    var player = root.TryGetProperty("player", out var p) ? p.GetString() : "unknown";
                    LoggerHelper.LogWarning($"Illegal move detected by AI - Player: {player}");

                    // Broadcast to WebSocket clients
                    await broadcastWs(new
                    {
                        type = "illegal_move",
                        game_id = root.TryGetProperty("game_id", out var gid) ? gid.GetString() : null,
                        player = player,
                        move = root.TryGetProperty("move", out var mv) ? JsonSerializer.Deserialize<object>(mv.GetRawText()) : null,
                        current_fen = root.TryGetProperty("current_fen", out var fen) ? fen.GetString() : null,
                        message = root.TryGetProperty("message", out var msg) ? msg.GetString() : null,
                        timestamp = DateTime.UtcNow
                    });
                    return;
                }

                // Robot response
                if (root.TryGetProperty("goal_id", out var goalId) && root.TryGetProperty("success", out var success))
                {
                    LoggerHelper.LogInfo($"Received response from robot: {goalId.GetString()}");
                    await broadcastWs(new
                    {
                        type = "robot_response",
                        success = success.GetBoolean(),
                        goal_id = goalId.GetString(),
                        response = message,
                        timestamp = DateTime.UtcNow
                    });
                    return;
                }

                // AI response with FEN and robot command
                if (root.TryGetProperty("fen_str", out var fenStr) && root.TryGetProperty("move", out var move))
                {
                    LoggerHelper.LogInfo("Received AI response with FEN and robot command");

                    // 1. Broadcast FEN to WebSocket clients
                    await broadcastWs(new
                    {
                        fen_str = fenStr.GetString(),
                        timestamp = DateTime.UtcNow,
                        source = "ai"
                    });

                    // 2. Create robot command and send to robot clients
                    var robotCommand = new
                    {
                        goal_id = $"ai_cmd_{DateTime.Now.Ticks.ToString().Substring(DateTime.Now.Ticks.ToString().Length - 6)}",
                        header = new
                        {
                            timestamp = DateTime.UtcNow,
                            source = "ai",
                            ai_id = clientInfo.AIId ?? "unknown"
                        },
                        move = JsonSerializer.Deserialize<object>(move.GetRawText())
                    };

                    await sendToRobots(robotCommand);

                    // 3. Broadcast AI move info
                    await broadcastWs(new
                    {
                        type = "ai_move_executed",
                        goal_id = robotCommand.goal_id,
                        move = robotCommand.move,
                        ai_id = clientInfo.AIId ?? "unknown",
                        timestamp = DateTime.UtcNow
                    });

                    // 4. Send acknowledgment
                    await writer.WriteLineAsync(JsonSerializer.Serialize(new
                    {
                        status = "ai_command_processed",
                        goal_id = robotCommand.goal_id,
                        message = "FEN broadcasted and robot command sent",
                        timestamp = DateTime.UtcNow
                    }));
                    return;
                }

                // FEN data
                if (root.TryGetProperty("fen_str", out fenStr) || root.TryGetProperty("fen", out fenStr))
                {
                    await broadcastWs(new
                    {
                        fen_str = fenStr.GetString(),
                        timestamp = DateTime.UtcNow,
                        source = "tcp"
                    });

                    await writer.WriteLineAsync(JsonSerializer.Serialize(new
                    {
                        status = "success",
                        message = "FEN received and broadcasted",
                        timestamp = DateTime.UtcNow
                    }));
                    return;
                }
            }
            catch (JsonException)
            {
                // Not JSON, try as plain FEN
                if (Regex.IsMatch(message, @"^[rnbqkpRNBQKP1-8\/\s\-]+$"))
                {
                    await broadcastWs(new
                    {
                        fen_str = message,
                        timestamp = DateTime.UtcNow,
                        source = "tcp"
                    });

                    await writer.WriteLineAsync(JsonSerializer.Serialize(new
                    {
                        status = "success",
                        message = "FEN received and broadcasted",
                        timestamp = DateTime.UtcNow
                    }));
                    return;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"Message processing error: {ex.Message}");
                await writer.WriteLineAsync(JsonSerializer.Serialize(new
                {
                    status = "error",
                    message = "Server processing error",
                    timestamp = DateTime.UtcNow
                }));
            }
        }

        public async Task ProcessWebSocketMessage(
            string message,
            WebSocket ws,
            Func<object, Task> sendToRobots,
            Func<object, Task> sendToAIs,
            Func<WebSocket, object, Task> sendWsMessage,
            Func<object, Task> broadcastWs)
        {
            try
            {
                var doc = JsonDocument.Parse(message);
                var root = doc.RootElement;

                // Robot command
                if (root.TryGetProperty("goal_id", out var goalId) && root.TryGetProperty("move", out var move))
                {
                    LoggerHelper.LogInfo($"Processing robot command: {goalId.GetString()}");
                    await sendToRobots(JsonSerializer.Deserialize<object>(message));

                    await sendWsMessage(ws, new
                    {
                        type = "command_sent",
                        goal_id = goalId.GetString(),
                        message = "Command sent to robot",
                        timestamp = DateTime.UtcNow
                    });
                }
                // AI request
                else if (root.TryGetProperty("type", out var type) && type.GetString() == "ai_request")
                {
                    LoggerHelper.LogInfo("Processing AI request");
                    await sendToAIs(JsonSerializer.Deserialize<object>(message));

                    await sendWsMessage(ws, new
                    {
                        type = "ai_request_sent",
                        request_id = root.GetProperty("request_id").GetString(),
                        message = "Request sent to AI",
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    await broadcastWs(JsonSerializer.Deserialize<object>(message));
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"WebSocket message parsing error: {ex.Message}");
                await sendWsMessage(ws, new
                {
                    type = "error",
                    message = "Invalid message format",
                    error = ex.Message
                });
            }
        }
    }
}
