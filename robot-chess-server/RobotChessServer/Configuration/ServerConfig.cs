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
        


        public ServerConfig()
        {
            PrimaryIP = "localhost";
            FallbackIP = "localhost";
            TcpPort = 8080;
            WebSocketPort = 8081;
            ConnectionTimeout = 3000;
            AlternativeTcpPorts = new[] { 8083, 8084 };
            AlternativeWebSocketPorts = new[] { 8085, 8086, 8087 };
            
        }

        public void PrintConfiguration()
        {
            Console.WriteLine("=== Robot Chess Integrated Server ===");
            Console.WriteLine($"Primary IP: {PrimaryIP}");
            Console.WriteLine($"Fallback IP: {FallbackIP}");
            Console.WriteLine($"TCP Port: {TcpPort} (alternatives: {string.Join(", ", AlternativeTcpPorts)})");
            Console.WriteLine($"WebSocket Port: {WebSocketPort} (alternatives: {string.Join(", ", AlternativeWebSocketPorts)})");
            
            Console.WriteLine("=====================================\n");
        }
    }
}
