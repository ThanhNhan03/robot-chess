using System.Net;
using System.Net.Sockets;

namespace RobotChessServer.Utilities
{
    /// <summary>
    /// Utilities cho các thao tác mạng
    /// </summary>
    public class NetworkUtils
    {
        /// <summary>
        /// Lấy IP address khả dụng
        /// </summary>
        public static async Task<IPAddress> GetAvailableIP(string primaryIP, string fallbackIP)
        {
            IPAddress ip = null;

            // Thử primary IP
            if (IPAddress.TryParse(primaryIP, out ip))
            {
                return ip;
            }

            if (primaryIP.ToLower() == "localhost" || primaryIP == "127.0.0.1")
            {
                return IPAddress.Loopback;
            }

            // Thử resolve hostname
            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(primaryIP);
                if (hostEntry.AddressList.Length > 0)
                {
                    return hostEntry.AddressList[0];
                }
            }
            catch { }

            // Fallback
            if (IPAddress.TryParse(fallbackIP, out ip))
            {
                return ip;
            }

            if (fallbackIP.ToLower() == "localhost" || fallbackIP == "127.0.0.1")
            {
                return IPAddress.Loopback;
            }

            // Default: bind to all interfaces
            return IPAddress.Any;
        }

        /// <summary>
        /// Find available port
        /// </summary>
        public static async Task<int> FindAvailablePort(IPAddress ip, int primaryPort, int[] altPorts)
        {
            if (await IsPortAvailable(ip, primaryPort))
                return primaryPort;

            Console.WriteLine($"Port {primaryPort} is in use, trying alternative ports...");

            foreach (var port in altPorts)
            {
                if (await IsPortAvailable(ip, port))
                {
                    Console.WriteLine($"Using alternative port: {port}");
                    return port;
                }
            }

            throw new Exception($"No available port found on {ip}");
        }

        /// <summary>
        /// Check if port is available
        /// </summary>
        public static async Task<bool> IsPortAvailable(IPAddress ip, int port)
        {
            try
            {
                var listener = new TcpListener(ip, port);
                listener.Start();
                listener.Stop();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
