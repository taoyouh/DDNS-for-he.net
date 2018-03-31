using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ddns
{
    static class IPHelper
    {
        public static IEnumerable<IPAddress> GetPublicIps()
        {
            return Dns.GetHostAddresses("")
                    .Where(address => address.IsPublicIp());
        }

        public static bool IsPublicIp(this IPAddress ip)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var bytes = ip.GetAddressBytes();
                if (bytes[0] == 10)
                    return false;
                if (bytes[0] == 172 && (bytes[1] & 0b11110000) == 16)
                    return false;
                if (bytes[0] == 192 && bytes[1] == 168)
                    return false;
                if (bytes[0] == 169 && bytes[1] == 254)
                    return false;
                if (bytes[0] == 127)
                    return false;
                if ((bytes[0] & 0b11110000) == 224)
                    return false;
                if ((bytes[0] & 0b11110000) == 240)
                    return false;
                if (bytes[0] == 255 && bytes[1] == 255 && bytes[2] == 255 && bytes[3] == 255)
                    return false;
                return true;
            }
            else if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                return !ip.IsIPv6LinkLocal && !ip.IsIPv6Multicast && !ip.IsIPv6SiteLocal;
            }
            else
            {
                return false;
            }
        }
    }
}
