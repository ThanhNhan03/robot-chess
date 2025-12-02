namespace RobotChessServer.Configuration
{
    /// <summary>
    /// Cấu hình cho Server
    /// </summary>
    public class ServerConfig
    {
        public string PrimaryIP { get; set; }
        public string FallbackIP { get; set; }
        public int TcpPort { get; set; }
        public int WebSocketPort { get; set; }
        public int ConnectionTimeout { get; set; }
        public int[] AlternativeTcpPorts { get; set; }
        public int[] AlternativeWebSocketPorts { get; set; }
        
        // Ngrok Configuration
        public bool UseNgrok { get; set; }
        public string NgrokTcpHost { get; set; }
        public int NgrokTcpPort { get; set; }
        public string NgrokWsHost { get; set; }
        public int NgrokWsPort { get; set; }

        public ServerConfig()
        {
            PrimaryIP = "localhost";
            FallbackIP = "localhost";
            TcpPort = 8080;
            WebSocketPort = 8081;
            ConnectionTimeout = 3000;
            AlternativeTcpPorts = new[] { 8083, 8084 };
            AlternativeWebSocketPorts = new[] { 8085, 8086, 8087 };
            
            // Ngrok defaults
            UseNgrok = false;
            NgrokTcpHost = "";
            NgrokTcpPort = 0;
            NgrokWsHost = "";
            NgrokWsPort = 0;
        }

        public void PrintConfiguration()
        {
            Console.WriteLine("=== Robot Chess Integrated Server ===");
            Console.WriteLine($"Primary IP: {PrimaryIP}");
            Console.WriteLine($"Fallback IP: {FallbackIP}");
            Console.WriteLine($"TCP Port: {TcpPort} (alternatives: {string.Join(", ", AlternativeTcpPorts)})");
            Console.WriteLine($"WebSocket Port: {WebSocketPort} (alternatives: {string.Join(", ", AlternativeWebSocketPorts)})");
            
            if (UseNgrok)
            {
                Console.WriteLine("\n--- Ngrok Configuration ---");
                Console.WriteLine($"TCP Tunnel: tcp://{NgrokTcpHost}:{NgrokTcpPort} -> localhost:{TcpPort}");
                Console.WriteLine($"WebSocket Tunnel: tcp://{NgrokWsHost}:{NgrokWsPort} -> localhost:{WebSocketPort}");
                Console.WriteLine("\nExternal Access:");
                Console.WriteLine($"  - Robot connects to: {NgrokTcpHost}:{NgrokTcpPort}");
                Console.WriteLine($"  - Web/Mobile connects to: ws://{NgrokWsHost}:{NgrokWsPort}");
            }
            
            Console.WriteLine("=====================================\n");
        }
    }
}
