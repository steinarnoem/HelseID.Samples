using HelseID.Clients.Owin.MvcHybrid;
using HelseID.Common.Clients;
using HelseID.Common.Jwt;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Configuration;

[assembly: OwinStartup(typeof(MVC_OWIN_Client.Startup))]

namespace MVC_OWIN_Client
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var settings = Config.Setting();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ResponseType = settings.ResponseType,
                SignInAsAuthenticationType = "Cookies",
                ClientId = settings.ClientId,
                ClientSecret = settings.ClientSecret,
                Authority = settings.Authority,
                RedirectUri = settings.RedirectUri,
                PostLogoutRedirectUri = settings.PostLogoutRedirectUri,
                Scope = settings.Scope,
                
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = async n =>
                        {
                            var opt = new HelseIdClientOptions
                            {
                                ClientId = n.Options.ClientId,
                                ClientSecret = n.Options.ClientSecret,
                                Authority = n.Options.Authority,
                                RedirectUri = settings.RedirectUri,
                                SigningMethod = (JwtGenerator.SigningMethod)Enum.Parse(typeof(JwtGenerator.SigningMethod), settings.SigningMethod),
                                CertificateThumbprint = settings.CertificateThumbprint
                            };

                            var client = new HelseIdClient(opt);

                            var tokenResponse = await client.AcquireTokenByAuthorizationCodeAsync(n.ProtocolMessage.Code);

                            //// create new identity
                            var id = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);
                            id.AddClaims(n.AuthenticationTicket.Identity.Claims);

                            id.AddClaim(new Claim("access_token", tokenResponse.AccessToken));
                            id.AddClaim(new Claim("expires_at", DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToLocalTime().ToString()));
                            id.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));
                            id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                            id.AddClaim(new Claim("sid", n.AuthenticationTicket.Identity.FindFirst("sid").Value));

                            n.AuthenticationTicket = new AuthenticationTicket(
                                new ClaimsIdentity(id.Claims, n.AuthenticationTicket.Identity.AuthenticationType),
                                n.AuthenticationTicket.Properties);
                        },

                    RedirectToIdentityProvider = redirectContext =>
                    {
                        var provider = settings.IdentityProvider;
                        var level = settings.OnlyLevel4 ? "Level4" : string.Empty;
                        var testPid = settings.TestPid;
                        var testHprNumber = settings.TestHprNumber;
                        var testSecurityLevel = settings.TestSecurityLevel;
                        var prompt = settings.ForceLogin ? "login" : string.Empty;

                        if (!string.IsNullOrWhiteSpace(provider))
                        {
                            redirectContext.ProtocolMessage.AcrValues = $"idp:{provider} {level}";
                        }
                        else if (!string.IsNullOrWhiteSpace(level))
                        {
                            redirectContext.ProtocolMessage.AcrValues = level;
                        }

                        if (!string.IsNullOrWhiteSpace(prompt))
                        {
                            redirectContext.ProtocolMessage.Prompt = "login";
                        }

                        if (!string.IsNullOrEmpty(testPid))
                        {
                            redirectContext.ProtocolMessage.SetParameter("test_pid", testPid);
                            if (!string.IsNullOrEmpty(testSecurityLevel))
                            {
                                redirectContext.ProtocolMessage.SetParameter("test_security_level", testSecurityLevel);
                            }
                        }

                        if (!string.IsNullOrEmpty(testHprNumber))
                        {
                            redirectContext.ProtocolMessage.SetParameter("test_hpr_number", testHprNumber);
                        }

                        return Task.CompletedTask;
                    }
                }
            });
        }
    }
}