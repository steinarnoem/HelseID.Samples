using HelseID.Clients.Common.Browser;
using HelseID.Clients.Common.Oidc;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using System;
using System.Threading.Tasks;
using static HelseID.Clients.Common.Jwt.JwtGenerator;

namespace HelseID.Clients.Common.Clients
{
    public class HelseIdClient
    {
        private readonly HelseIdClientOptions _options;

        public HelseIdClient(HelseIdClientOptions options)
        {
            _options = options;
        }

        public async Task<LoginResult> Login()
        {
            if (_options.SigningMethod == SigningMethod.X509EnterpriseSecurityKey && string.IsNullOrEmpty(_options.CertificateThumbprint))
            {
                throw new ArgumentNullException("To use enterprise certificate as client authentication please provide a valid thumbprint");
            }

            if (_options.Browser == null)
            {
                _options.Browser = new SystemBrowser(_options.RedirectUri);
            }

            var oidcClient = new OidcClient(_options);

            var disco = await OidcDiscoveryHelper.GetDiscoveryDocument(_options.Authority);
            if (disco.IsError) throw new Exception(disco.Error);

            var result = await oidcClient.LoginAsync(new LoginRequest()
            {
                BackChannelExtraParameters = GetBackChannelExtraParameters(disco),
                FrontChannelExtraParameters = GetFrontChannelExtraParameters()
            });

            return result;
        }

        public async Task<TokenResponse> ClientCredentialsSignIn()
        {
            var disco = await OidcDiscoveryHelper.GetDiscoveryDocument(_options.Authority);
            if (disco.IsError) throw new Exception(disco.Error);

            var extraParams = GetBackChannelExtraParameters(disco);
            var c = new TokenClient(disco.TokenEndpoint, _options.ClientId, _options.ClientSecret);
            var result = await c.RequestClientCredentialsAsync(_options.Scope, extraParams);

            return result;
        }

        private object GetBackChannelExtraParameters(DiscoveryResponse disco, string token = null)
        {
 
            ClientAssertion assertion = null;
            if (_options.SigningMethod == SigningMethod.RsaSecurityKey)
            {
                assertion = ClientAssertion.CreateWithRsaKeys(_options.ClientId, disco.TokenEndpoint);
            }
            if (_options.SigningMethod == SigningMethod.X509EnterpriseSecurityKey)
            {
                assertion = ClientAssertion.CreateWithEnterpriseCertificate(_options.ClientId, disco.TokenEndpoint, _options.CertificateThumbprint);
            }

            var payload = new
            {
                token,
                assertion?.client_assertion,
                assertion?.client_assertion_type,
            };
            return payload;
        }

        private object GetFrontChannelExtraParameters()
        {
            var preselectIdp = _options.PreselectIdp;

            if (string.IsNullOrEmpty(preselectIdp))
                return null;

            return new { acr_values = preselectIdp, prompt = "Login" };
        }

        public async Task<TokenResponse> TokenExchange(string accessToken)
        {
            var disco = await DiscoveryClient.GetAsync(_options.Authority);
            if (disco.IsError) throw new Exception(disco.Error);
            var client = new TokenClient(disco.TokenEndpoint, _options.ClientId, _options.ClientSecret);

            var payload = GetBackChannelExtraParameters(disco, accessToken);

            // send custom grant to token endpoint, return response
            var response = await client.RequestCustomGrantAsync("token_exchange", _options.Scope, payload);

            return response;
        }
    }
}
