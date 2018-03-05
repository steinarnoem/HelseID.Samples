using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HelseID.Clients.HIDEnabler.Models;
using HelseID.Clients.HIDEnabler.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HelseID.Clients.HIDEnabler.Services
{
    public class KjernejournalService
    {
        private readonly string _baseAddress;
        private readonly string _token;

        /// <summary>
        /// Call KJ related apis
        /// </summary>
        /// <param name="baseAddress">Base address of API</param>
        /// <param name="token">Token containing KJ scope</param>
        public KjernejournalService(string baseAddress, string token)
        {
            _baseAddress = baseAddress;
            _token = token;
        }


        public async Task<Organization> ShowAndCollectOrgNumber()
        {

            // Get list of orgnumbers from KJ API
            var orgs = await GetOrgNumbers();

            // Let user choose orgnumber and return
            return ChooseOrgNumber(orgs);

        }


        public Organization ChooseOrgNumber(List<Organization> orgs)
        {

            Console.WriteLine("");
            Console.WriteLine("Choose an orgnr");
            Console.WriteLine("--------------");


            ListAll(orgs);

            return ChooseOrg(orgs);

        }


        public Organization ChooseOrg(List<Organization> orgs)
        {
            Organization org;
            while (!HandleInput(orgs, out org)) { }

            return org;

        }

        private bool HandleInput(List<Organization> orgs, out Organization org)
        {
            var maxNumber = orgs.Count;
            org = null;

            Console.WriteLine($"Choose a number (1-{maxNumber}). R = repeat list");
            var info = Console.ReadLine();
            if (info == "R" || info == "r")
            {
                ListAll(orgs);
                return false;
            }

            int.TryParse(info, out var chosenNumber);

            if (chosenNumber < 1 || chosenNumber > maxNumber)
            {
                Console.WriteLine(string.Empty);
                Console.WriteLine($"Invalid choice: {info}");
                Console.WriteLine(string.Empty);
                return false;
            }

            org = orgs[chosenNumber - 1];
            return true;

        }

        public void ListAll(List<Organization> orgs)
        {
            Console.WriteLine($"Console height: {Console.WindowHeight}");
            for (var i = 0; i < orgs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {orgs[i].Nr} - {orgs[i].Name}");

                if ((i + 1) % (Console.WindowHeight - 5) != 0)
                    continue;

                Console.WriteLine("Press any key to continue");
                Console.ReadKey(false);
            }
        }



        public async Task<List<Organization>> GetOrgNumbers()
        {
            // TODO: WHEN API IS FINISHED
            //var httpClient = HttpUtil.GetHttpClient(_token, _baseAddress);
            //var response = await httpClient.GetAsync("/kjgetorgnrs/");
            //var responseAsJson = await response.Content.ReadAsStringAsync();

            //var orgs = JsonConvert.DeserializeObject<OrganizationsResponse>(responseAsJson);
            //return orgs.Organizations;


            const int maxNr = 50;
            var res = new List<Organization>();
            var curr = 1;

            while (curr < maxNr)
            {
                res.Add(new Organization { Nr = "O" + curr, Name = "Org" + curr });
                curr++;
            }

            return res;
        }



    }
}
