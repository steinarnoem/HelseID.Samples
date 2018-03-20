
using HelseID.Clients.HIDEnabler.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HelseID.Common.Extensions;
using HelseID.Models.KJ;

namespace HelseID.Clients.HIDEnabler
{
    public class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        public static async Task MainAsync()
        {
            var settings = InitSettings();

            var dcrService = new DcrService(settings);
            var kjService = new KjernejournalService(settings);
            var signinService = new SigninService(settings);

            Console.WriteLine("+-----------------------+");
            Console.WriteLine("|      HIDEnabler       |");
            Console.WriteLine("+-----------------------+");
            Console.WriteLine("");

            Console.WriteLine("Signing in with enterprise certificate");
            var accessToken = await signinService.SignIn();

            Console.WriteLine("AccessToken: ");
            Console.WriteLine(accessToken.DecodeToken());

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            Console.WriteLine("Creating dcr client");

            var client = await dcrService.CreateClient(accessToken, settings.GrantType, settings.RedirectUri, settings.LogoutUri, settings.Scopes.FromSpaceSeparatedToList());

            var clientId = client.ClientId;
            Console.WriteLine("Created client with dcr api:");
            Console.WriteLine("Clientid:     " + client.ClientId);
            Console.WriteLine("GrantType:    " + string.Join(",", client.GrantTypes.ToList()));
            Console.WriteLine("RedirectUris: " + string.Join(",", client.RedirectUris));
            Console.WriteLine("Scopes:       " + string.Join(",", client.AllowedScopes));

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            var orgNumbers = await kjService.GetOrgNumbers(accessToken);

            // Let user choose orgnumber and return
            var orgNumber = ChooseOrgNumber(orgNumbers);

            await kjService.SetOrgNumber(accessToken, clientId, orgNumber.Nr);

            Console.Clear();
            Console.WriteLine("Signing in with dcr client");
            //Try to log in with the new configuration
            var loginResponse = await signinService.RsaSignInWithAuthCode(clientId, settings.Scopes);

            Console.WriteLine("Signin complete. Recieved access token:");
            Console.WriteLine(loginResponse.AccessToken.DecodeToken());

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static Settings InitSettings()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("Settings/appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var settings = new Settings
            {
                Authority = configuration.GetSection("Authority")?.Value,
                DcrApi = configuration.GetSection("DcrApi")?.Value,
                Thumbprint = configuration.GetSection("Thumbprint")?.Value,
                ClientId = configuration.GetSection("ClientId")?.Value,

                GrantType = configuration.GetSection("GrantType")?.Value,
                RedirectUri = configuration.GetSection("RedirectUri")?.Value,
                LogoutUri = configuration.GetSection("LogoutUri")?.Value,
                Scopes = configuration.GetSection("Scopes")?.Value,
            };
            return settings;
        }

        public static Organization ChooseOrgNumber(List<Organization> orgs)
        {
            Console.WriteLine("");
            Console.WriteLine("Choose an orgnr");
            Console.WriteLine("--------------");

            ListAll(orgs);

            return ChooseOrg(orgs);
        }

        public static Organization ChooseOrg(List<Organization> orgs)
        {
            Organization org;
            while (!HandleInput(orgs, out org)) { }

            return org;
        }

        private static bool HandleInput(List<Organization> orgs, out Organization org)
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

        public static void ListAll(List<Organization> orgs)
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
    }
}