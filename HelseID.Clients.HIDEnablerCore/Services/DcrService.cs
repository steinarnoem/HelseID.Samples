using HelseID.Clients.Common.Crypto;
using HelseID.Clients.HIDEnabler.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HelseID.Clients.HIDEnabler.Services
{
    internal class DcrService
    {
        private readonly Settings _settings;

        public DcrService(Settings settings)
        {
            _settings = settings;
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
                    Type = "private_key_jwt:RsaPrivateKeyJwtSecret",
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

            var client = await StoreClient(clientRequest, accessToken);

            return client;
        }

        private async Task<ClientResponse> StoreClient(ClientRequest request, string token)
        {
            var httpClient = GetHttpClient(token);

            var requestAsJson = JsonConvert.SerializeObject(request);
            var content = new StringContent(requestAsJson, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("api/connect/client/register", content);

            var responseAsJson = await response.Content.ReadAsStringAsync();
            var client = JsonConvert.DeserializeObject<ClientResponse>(responseAsJson);

            return client;
        }

        private HttpClient GetHttpClient(string token)
        {
            var baseAddress = _settings.DcrApi;

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);
            return client;
        }
    }
}
