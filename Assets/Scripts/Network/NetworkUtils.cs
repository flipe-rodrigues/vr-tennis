using System.Net;
using System.Net.Sockets;

public class NetworkUtils
{
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString(); // Found LAN IPv4
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}
