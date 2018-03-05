using HelseID.Clients.Common.Clients;
using HelseID.Clients.HIDEnabler.Models;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using System.Threading.Tasks;

namespace HelseID.Clients.HIDEnabler.Services
{
    public class SigninService
    {
        private readonly Settings _settings;

        public SigninService(Settings settings)
        {
            _settings = settings;
        }
        public async Task<string> SignIn()
        {
            var options = new HelseIdClientOptions
            {
                Authority = _settings.Authority,
                ClientId = _settings.ClientId,
                RedirectUri = _settings.RedirectUri,
                Scope = "openid profile helseid://scopes/client/dcr nhn/kj",
                FilterClaims = false,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                CertificateThumbprint = _settings.Thumbprint,
                SigningMethod = Common.Jwt.JwtGenerator.SigningMethod.X509EnterpriseSecurityKey
            };

            var client = new HelseIdClient(options);
            var result = await client.Login();

            return result.AccessToken;
        }

        public async Task<TokenResponse> RsaSignIn(string clientId, string scope)
        {
            var options = new HelseIdClientOptions
            {
                Authority = _settings.Authority,
                ClientId = clientId,
                RedirectUri = _settings.RedirectUri,
                Scope = scope,
                FilterClaims = false,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                CertificateThumbprint = _settings.Thumbprint,
                SigningMethod = Common.Jwt.JwtGenerator.SigningMethod.RsaSecurityKey
            };
            var c = new HelseIdClient(options);

            var result = await c.ClientCredentialsSignIn();

            return result;
        }

        public async Task<LoginResult> RsaSignInWithAuthCode(string clientId, string scope)
        {
            var options = new HelseIdClientOptions
            {
                Authority = _settings.Authority,
                ClientId = clientId,
                RedirectUri = _settings.RedirectUri,
                Scope = scope,
                FilterClaims = false,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                SigningMethod = Common.Jwt.JwtGenerator.SigningMethod.RsaSecurityKey
            };

            var oidcClient = new HelseIdClient(options);

            var result = await oidcClient.Login();
            return result;
        }
    }
}
