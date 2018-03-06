using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HelseID.Common.Oidc;
using HelseID.Common.X509Certificates;
using IdentityModel.OidcClient;
using Microsoft.Extensions.Logging;

namespace HelseID.Clients.HIDEnabler.Utils
{
    internal class SignIn
    {
        private readonly string _customUriScheme;
        private readonly ILogger<SignIn> _logger;
        private string _x509CertThumbprint;

        public SignIn(ILoggerFactory loggerFactory, string customUriScheme, string x509CertThumbprint)
        {
            _customUriScheme = customUriScheme;
            _logger = loggerFactory.CreateLogger<SignIn>();
            _x509CertThumbprint = x509CertThumbprint;
        }

        public LoginResult Result { get; private set; } 

        public async Task Run()
        {
            _logger.LogInformation("Signing in....");

            // create a redirect URI using the custom redirect uri
            var redirectUri = string.Format(_customUriScheme + "://callback");
            _logger.LogDebug("redirect URI: " + redirectUri);

            var options = new OidcClientOptions
            {
                Authority = "https://demo.identityserver.io",
                ClientId = "native.code",
                Scope = "openid profile api",
                RedirectUri = redirectUri,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect
            };

            var client = new OidcClient(options);
            var state = await client.PrepareLoginAsync();

            _logger.LogDebug($"Start URL: {state.StartUrl}");

            var callbackManager = new CallbackManager(state.State);

            // open system browser to start authentication
            Process.Start(state.StartUrl);

            _logger.LogDebug("Running callback manager");
            var response = await callbackManager.RunServer();


            _logger.LogDebug($"Response from authorize endpoint: {response}");
            _logger.LogInformation("Authentication successful");

            // Brings the Console to Focus.
            ConsoleUtil.BringConsoleToFront();

            var discoveryDocument = await OidcDiscoveryHelper.GetDiscoveryDocument(client.Options.Authority);


            try
            {
                //TODO: hent x509CertThumbprint
                var enterpriseCertAssertion = ClientAssertion.CreateWithEnterpriseCertificate(client.Options.ClientId,
                    discoveryDocument.TokenEndpoint, _x509CertThumbprint);
                _logger.LogDebug($"Got enterprise cert assertion: {enterpriseCertAssertion}");

                Result = await client.ProcessResponseAsync(response, state, enterpriseCertAssertion);
                ConsoleUtil.BringConsoleToFront();

                if (Result.IsError)
                {
                    _logger.LogError("\n\nError when signing in:\n{0}", Result.Error);
                }
                else
                {
                    _logger.LogDebug("\n\nUser Claims:");
                    foreach (var claim in Result.User.Claims)
                    {
                        _logger.LogDebug("{0}: {1}", claim.Type, claim.Value);
                    }

                    _logger.LogDebug("");
                    _logger.LogDebug("Received Access token for dcr:\n{0}", Result.AccessToken);

                    if (!string.IsNullOrWhiteSpace(Result.RefreshToken))
                    {
                        _logger.LogDebug("Refresh token:\n{0}", Result.RefreshToken);
                    }
                }
            }
            catch (UnknownX509CertificateThumbprintException unknownThumbrintException)
            {
                Console.WriteLine($"Could not get client assertion signed with enterprise certificate.{Environment.NewLine}ClientID: {client.Options.ClientId}");
                Console.WriteLine(unknownThumbrintException.Message);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occurred. {e.Message}");
                throw;
            }
        }

        //public static string GetRequestPostData(HttpListenerRequest request)
        //{
        //    if (!request.HasEntityBody)
        //    {
        //        return null;
        //    }

        //    using (var body = request.InputStream)
        //    {
        //        using (var reader = new System.IO.StreamReader(body, request.ContentEncoding))
        //        {
        //            return reader.ReadToEnd();
        //        }
        //    }
        //}
    }
}
