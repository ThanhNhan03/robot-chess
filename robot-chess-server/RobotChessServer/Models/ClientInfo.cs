using System.Net.Sockets;

namespace RobotChessServer.Models
{
    /// <summary>
    /// Thông tin client TCP
    /// </summary>
    public class ClientInfo
    {
        public TcpClient Client { get; set; }
        public bool IsRobot { get; set; }
        public bool IsAI { get; set; }
        public string RobotId { get; set; }
        public string AIId { get; set; }
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            if (IsRobot)
                return $"Robot({RobotId})";
            if (IsAI)
                return $"AI({AIId})";
            return "Unknown";
        }
    }
}
