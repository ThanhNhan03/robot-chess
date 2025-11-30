namespace RobotChessServer.Configuration
{
    /// <summary>
    /// Quản lý cấu hình từ environment variables
    /// </summary>
    public class ConfigurationManager
    {
        public static ServerConfig LoadConfiguration()
        {
            var config = new ServerConfig
            {
                PrimaryIP = Environment.GetEnvironmentVariable("PRIMARY_IP") ?? "localhost",
                FallbackIP = Environment.GetEnvironmentVariable("FALLBACK_IP") ?? "localhost",
                TcpPort = int.Parse(Environment.GetEnvironmentVariable("TCP_PORT") ?? "8080"),
                WebSocketPort = int.Parse(Environment.GetEnvironmentVariable("WS_PORT") ?? "8081"),
                ConnectionTimeout = int.Parse(Environment.GetEnvironmentVariable("CONNECTION_TIMEOUT") ?? "3000"),
                AlternativeTcpPorts = ParsePorts(Environment.GetEnvironmentVariable("ALT_TCP_PORTS"), new[] { 8083, 8084 }),
                AlternativeWebSocketPorts = ParsePorts(Environment.GetEnvironmentVariable("ALT_WS_PORTS"), new[] { 8085, 8086, 8087 }),
                
                // Ngrok configuration
                UseNgrok = bool.Parse(Environment.GetEnvironmentVariable("USE_NGROK") ?? "false"),
                NgrokTcpHost = Environment.GetEnvironmentVariable("NGROK_TCP_HOST") ?? "",
                NgrokTcpPort = int.Parse(Environment.GetEnvironmentVariable("NGROK_TCP_PORT") ?? "0"),
                NgrokWsHost = Environment.GetEnvironmentVariable("NGROK_WS_HOST") ?? "",
                NgrokWsPort = int.Parse(Environment.GetEnvironmentVariable("NGROK_WS_PORT") ?? "0")
            };

            return config;
        }

        private static int[] ParsePorts(string portsStr, int[] defaultPorts)
        {
            if (string.IsNullOrEmpty(portsStr))
                return defaultPorts;

            return portsStr.Split(',').Select(p => int.Parse(p.Trim())).ToArray();
        }
    }
}
