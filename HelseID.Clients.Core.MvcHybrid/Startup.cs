using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using HelseID.Common.Clients;
using HelseID.Common.Jwt;
using HelseID.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading.Tasks;

namespace HelseID.Clients.Core.MvcHybrid
{
    public class Startup
    {
        public static IConfigurationRoot Configuration { get; private set; }
        public Startup()
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }


        public void ConfigureServices(IServiceCollection services)
        {
            var settings = Config.Setting();

            services.AddMvc();

            var opt = new HelseIdClientOptions
            {
                ClientId = settings.ClientId,
                ClientSecret = settings.ClientSecret,
                Authority = settings.Authority,
                RedirectUri =settings.RedirectUri,
                PostLogoutRedirectUri = settings.PostLogoutRedirectUri,
                SigningMethod = (JwtGenerator.SigningMethod)Enum.Parse(typeof(JwtGenerator.SigningMethod), settings.SigningMethod),
                CertificateThumbprint = settings.CertificateThumbprint,
                Scope = settings.Scope
            };
            var client = new HelseIdClient(opt);

            services.AddSingleton<IHelseIdClient>(client);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.Cookie.Name = "HelseID.Clients.Core.MvcHybrid";
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = settings.Authority;
                    options.RequireHttpsMetadata = true;
                    options.ClientSecret = settings.ClientSecret;
                    options.ClientId = settings.ClientId;
                    options.ResponseType = settings.ResponseType;
                    options.Scope.Clear();
                    foreach (var scope in settings.Scope.FromSpaceSeparatedToList())
                    {
                        options.Scope.Add(scope);
                    }

                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;
                    options.Events = new OpenIdConnectEvents
                    {
                        OnAuthorizationCodeReceived = async ctx =>
                        {
                            var result = await client.AcquireTokenByAuthorizationCodeAsync(ctx.ProtocolMessage.Code);

                            var response = new OpenIdConnectMessage
                            {
                                AccessToken = result.AccessToken,
                                IdToken = result.IdentityToken,
                                RefreshToken = result.RefreshToken
                            };

                            ctx.HandleCodeRedemption(response);
                        },
                        OnRedirectToIdentityProvider = redirectContext =>
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
                    };
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}
