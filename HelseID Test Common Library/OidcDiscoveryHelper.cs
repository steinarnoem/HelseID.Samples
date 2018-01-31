﻿using System.Threading.Tasks;
using IdentityModel.Client;

namespace HelseID.Clients.Common
{
    public class OidcDiscoveryHelper
    {
        public static async Task<DiscoveryResponse> GetDiscoveryDocument(string authority)
        {
            var discoClient = new DiscoveryClient(authority);
            var discoveryResponse = await discoClient.GetAsync();

            return discoveryResponse;
        }
    }
}
