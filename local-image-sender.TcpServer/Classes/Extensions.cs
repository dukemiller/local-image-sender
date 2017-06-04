using System.Net;
using System.Net.Sockets;

namespace local_image_sender.TcpServer.Classes
{
    public static class Extensions
    {
        public static string IpAddress(this TcpClient client)
        {
            return ((IPEndPoint)client.Client.RemoteEndPoint).Address.MapToIPv4().ToString();
        }
    }
}