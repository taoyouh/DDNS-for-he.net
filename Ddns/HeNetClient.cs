using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Ddns
{
    class HeNetClient
    {
        public static async Task<(bool ok, string message)> UpdateIpAsync(string hostname, string password, string ip)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["hostname"] = hostname,
                        ["password"] = password,
                        ["myip"] = ip
                    });
                var response = await client.PostAsync(new Uri("https://dyn.dns.he.net/nic/update"), content);
                if (response.StatusCode == HttpStatusCode.Ok)
                    return (true, "");
                else
                    return (false, await response.Content.ReadAsStringAsync());
            }
        }
    }
}
