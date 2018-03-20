using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using HelseID.Common.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using HelseID.Common.Clients;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace HelseID.Clients.Core.MvcHybrid.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHelseIdClient _client;

        public HomeController(IHelseIdClient client)
        {
            _client = client;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secure()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> CallApi()
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            var client = new HttpClient();
            client.SetBearerToken(token);

            var result = await client.GetStringAsync("https://hid-testapi-umla.azurewebsites.net/Api/ReturnTokenContent");
            ViewBag.Json = JArray.Parse(result.ToString());

            return View();
        }

        [Authorize]
        public async Task<IActionResult> TokenExchange()
        {
            var at = await HttpContext.GetTokenAsync("access_token");
            var tokenResponse = await _client.TokenExchange(at);

            if (!tokenResponse.IsError)
            {
                ViewBag.Token = tokenResponse.AccessToken;
                ViewBag.Jwt = tokenResponse.AccessToken.DecodeToken();

                return View();
            }

            ViewData["Error"] = tokenResponse.Error;
            return View("Error");
        }

        public async Task<IActionResult> RenewTokens()
        {
            var rt = await HttpContext.GetTokenAsync("refresh_token");
            var tokenResult = await _client.AcquireTokenByRefreshToken(rt);

            if (!tokenResult.IsError)
            {
                var old_id_token = await HttpContext.GetTokenAsync("id_token");
                var new_access_token = tokenResult.AccessToken;
                var new_refresh_token = tokenResult.RefreshToken;

                var tokens = new List<AuthenticationToken>();
                tokens.Add(new AuthenticationToken { Name = OpenIdConnectParameterNames.IdToken, Value = old_id_token });
                tokens.Add(new AuthenticationToken { Name = OpenIdConnectParameterNames.AccessToken, Value = new_access_token });
                tokens.Add(new AuthenticationToken { Name = OpenIdConnectParameterNames.RefreshToken, Value = new_refresh_token });

                var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
                tokens.Add(new AuthenticationToken { Name = "expires_at", Value = expiresAt.ToString("o", CultureInfo.InvariantCulture) });

                var info = await HttpContext.AuthenticateAsync("Cookies");
                info.Properties.StoreTokens(tokens);
                await HttpContext.SignInAsync("Cookies", info.Principal, info.Properties);

                return Redirect("~/Home/Secure");
            }

            ViewData["Error"] = tokenResult.Error;
            return View("Error");
        }

        public IActionResult Logout()
        {
            return new SignOutResult(new[] { "Cookies", "oidc" });
        }


        public IActionResult Error()
        {
            return View();
        }
    }
}
