using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelseID.Test.WPF.Common
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
