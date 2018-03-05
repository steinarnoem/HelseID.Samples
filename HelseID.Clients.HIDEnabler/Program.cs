using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
//using HelseID.Clients.HIDEnabler.Services;
using HelseID.Clients.HIDEnabler.Utils;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HelseID.Clients.HIDEnabler
{
    class Program
    {
        private static string _logFile;
        private static ILoggerFactory _loggerFactory;
        private static ILogger<Program> _logger;
        private static IConfigurationRoot _configuration;
        private const string CustomUriScheme = "helseid-enabler-client";

        static async Task Main(string[] args)
        {


            if (args.Any())
            {
                await ProcessCallback(args[0]);
            }
            else
            {
                ConfigureServices();
                await Run();
            }

        }

        private static async Task ProcessCallback(string args)
        {
            var response = new AuthorizeResponse(args);
            if (!string.IsNullOrWhiteSpace(response.State))
            {
                Console.WriteLine($"Found state: {response.State}");
                var callbackManager = new CallbackManager(response.State);
                await callbackManager.RunClient(args);
            }
            else
            {
                Console.WriteLine("Error: no state on response");
            }
        }

        private static void ConfigureServices()
        {
            var configBuilder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("settings/appsettings.json");

            _configuration = configBuilder.Build();

            _logFile = $"c:\\temp\\HIDEnabler_Log_{DateTime.Now.Ticks}.txt";
            _loggerFactory = new LoggerFactory()
                .AddFile(_logFile, LogLevel.Trace)
                .AddConsole(LogLevel.Information, false);

            _logger = _loggerFactory.CreateLogger<Program>();
        }

        private static async Task Run()
        {

            new RegistryConfig(CustomUriScheme).Configure();

            Console.WriteLine("+-------------------------+");
            Console.WriteLine("|  Welcome to HID Enabler |");
            Console.WriteLine("+-------------------------+");
            Console.WriteLine("");
            Console.WriteLine("");

            var thumbprint = GetCertificateThumbprint();
            
            Console.WriteLine("Press any key to sign in...");
            Console.ReadKey();


            // TODO: Verify key generation, read of enterprise cert + more?
            var signin = new SignIn(_loggerFactory, CustomUriScheme, thumbprint);
            await signin.Run();

            if (signin.Result?.AccessToken == null)
            {
                Console.WriteLine($"Failed to authenticate properly. See logfile {_logFile}");
                if (!string.IsNullOrWhiteSpace(signin?.Result?.Error))
                    _logger.LogError($"Signin error: {signin.Result.Error}");
                return;
            }

            //var dcr = new DcrService();
            //dcr.CreateClientConfiguration();

            //var kj = new KjernejournalService(_configuration["BaseAddress"], "alskdhføalskdfjøa"); //signin.Result.AccessToken);
            //var orgnr = kj.ShowAndCollectOrgNumber();
            //kj.SetOrgNr(kj);

            //dcr.DumpClientConfiguration();

            //_logger.LogInformation("Client configuration successfully created");

            //Console.ReadKey();
        }

        private static string GetCertificateThumbprint()
        {
            while (true)
            {
                Console.WriteLine("Enter certificate thumbprint (press Enter to continue):");
                var thumbprint = Console.ReadLine();
                Console.WriteLine("");

                Console.WriteLine($"You entered: {thumbprint}");
                Console.WriteLine("");
                Console.WriteLine("Is the thumprint correct? (Y/N)");

                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Y)
                {
                    return thumbprint;
                }

                Console.WriteLine("");
                Console.WriteLine("You indicated that the thumbprint was not correct - let's try again :)");
                Console.WriteLine("");
            }            
        }
    }
}
