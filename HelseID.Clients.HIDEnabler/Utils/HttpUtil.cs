using System;
using System.Net.Http;
using Serilog.Core;

namespace HelseID.Clients.HIDEnabler.Utils
{
    public class HttpUtil
    {

        public static HttpClient GetHttpClient(string token, string baseAddress)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);
            return client;
        }

    }
}
