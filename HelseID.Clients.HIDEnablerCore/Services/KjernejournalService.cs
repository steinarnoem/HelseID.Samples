using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HelseID.Clients.HIDEnabler.Models;
using Newtonsoft.Json;

namespace HelseID.Clients.HIDEnabler.Services
{
    public class KjernejournalService
    {
        private readonly Settings _settings;

        /// <summary>
        /// Call KJ related apis
        /// </summary>
        /// <param name="baseAddress">Base address of API</param>
        /// <param name="token">Token containing KJ scope</param>
        public KjernejournalService(Settings settings)
        {
            _settings = settings;
        }

        public async Task<List<Organization>> GetOrgNumbers(string accessToken)
        {
            var httpClient = GetHttpClient(accessToken);
            var response = await httpClient.GetAsync("api/kjorgnr/");
            var responseAsJson = await response.Content.ReadAsStringAsync();

            var orgs = JsonConvert.DeserializeObject<List<Organization>>(responseAsJson);
            return orgs;
        }

        public async Task<bool> SetOrgNumber(string accessToken, string clientId, string orgNumber)
        {
            var httpClient = GetHttpClient(accessToken);

            var vm = new KjOrgNrViewModel
            {
                ClientId = clientId,
                Orgnr = orgNumber
            };

            var content = new StringContent(JsonConvert.SerializeObject(vm), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("api/kjorgnr/", content);

            return response.IsSuccessStatusCode;
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
