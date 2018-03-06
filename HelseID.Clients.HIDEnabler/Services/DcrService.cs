using HelseID.Common.Clients;
using HelseID.Common.Crypto;
using HelseID.Models;
using HelseID.Models.DCR;
using HelseID.Models.DCR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelseID.Clients.HIDEnabler.Services
{
    public class DcrService
    {
        private DcrClient _client;

        public DcrService(Settings settings)
        {
            _client = new DcrClient(settings.DcrApi);
        }

        public async Task<ClientResponse> CreateClient(string accessToken, string grantType, string redirectUri, string logoutUri, string[] allowedScopes)
        {
            var client = await CreateClient(accessToken, new List<string> { grantType }, new List<string> { redirectUri }, logoutUri, allowedScopes);

            return client;
        }

        public async Task<ClientResponse> CreateClient(string accessToken, List<string> grantTypes, List<string> redirectUris, string logoutUri, string[] allowedScopes)
        {
            var clientRequest = new ClientRequest
            {
                ClientName = "Dcr created client" + Guid.NewGuid(),
                Secrets = new[] {
                new Secret{
                    Type = SecretTypes.RsaPrivateKey,
                    Value = RSAKeyGenerator.CreateNewKey(false)
                    }
                },
                RequireClientSecret = true,
                AlwaysSendClientClaims = true,
                GrantTypes = grantTypes,
                RedirectUris = redirectUris,
                LogoutUri = logoutUri,
                AllowedScopes = allowedScopes
            };

            _client.SetBearerToken(accessToken);
            return await _client.StoreClient(clientRequest);
        }
    }
}
