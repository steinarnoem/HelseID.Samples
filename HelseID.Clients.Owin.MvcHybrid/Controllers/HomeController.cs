using HelseID.Common.Clients;
using HelseID.Common.Jwt;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using HelseID.Common.Extensions;
using Newtonsoft.Json;
using System.Web.Configuration;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVC_OWIN_Client.Controllers
{
    public class HomeController : Controller
    {
        private IHelseIdClient _client;

        public HomeController()
        {
            var opt = new HelseIdClientOptions
            {
                ClientId = WebConfigurationManager.AppSettings["ClientId"],
                ClientSecret = WebConfigurationManager.AppSettings["ClientSecret"],
                Authority = WebConfigurationManager.AppSettings["Authority"],
                RedirectUri = WebConfigurationManager.AppSettings["RedirectUri"],
                SigningMethod = (JwtGenerator.SigningMethod)Enum.Parse(typeof(JwtGenerator.SigningMethod), WebConfigurationManager.AppSettings["SigningMethod"]),
                CertificateThumbprint = WebConfigurationManager.AppSettings["CertificateThumbprint"],
                PreselectIdp = WebConfigurationManager.AppSettings["PreselectIdp"],
            };
            _client = new HelseIdClient(opt);
        }
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Claims()
        {
            ViewBag.Message = "Claims";

            var cp = (ClaimsPrincipal)User;
            ViewData["access_token"] = cp.FindFirst("access_token").Value;

            return View();
        }

        [Authorize]
        public async Task<ActionResult> CallApi()
        {
            var token = (User as ClaimsPrincipal).FindFirst("access_token").Value;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
            // client.SetBearerToken(token);

            var result = await client.GetStringAsync("https://hid-testapi-umla.azurewebsites.net/Api/ReturnTokenContent");
            ViewBag.Json = JArray.Parse(result.ToString());

            return View();
        }
        public async Task<ActionResult> RenewTokens()
        {
            var cp = (ClaimsPrincipal)User;
            var rt = cp.FindFirst("refresh_token").Value;
            var tokenResponse = await _client.AcquireTokenByRefreshToken(rt);

            if (!tokenResponse.IsError)
            {
                var id = new ClaimsIdentity(cp.Identity.AuthenticationType);
                var result = from claim in cp.Claims
                             where claim.Type != "access_token" && claim.Type != "refresh_token" && claim.Type != "expires_at"
                             select claim;
                id.AddClaims(result);
                id.AddClaim(new Claim("access_token", tokenResponse.AccessToken));
                id.AddClaim(new Claim("expires_at", DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToLocalTime().ToString()));
                id.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));

                HttpContext.GetOwinContext().Authentication.SignIn(new ClaimsIdentity(id.Claims, cp.Identity.AuthenticationType));

                return Redirect("~/Home/Claims");
            }

            ViewData["Error"] = tokenResponse.Error;
            return View("Error");
        }

        [Authorize]
        public async Task<ActionResult> TokenExchange()
        {
            var cp = (ClaimsPrincipal)User;
            var at = cp.FindFirst("access_token").Value;
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

        public async Task<ActionResult> Signout()
        {
            // also possible to pass post logout redirect url via properties
            //var properties = new AuthenticationProperties
            //{
            //    RedirectUri = "http://localhost:2672/"
            //};

            // await _client.Logout();

            Request.GetOwinContext().Authentication.SignOut();
            return Redirect("/");
        }

        [AllowAnonymous]
        public void OidcSignOut(string sid)
        {
            var cp = (ClaimsPrincipal)User;
            var sidClaim = cp.FindFirst("sid");
            if (sidClaim != null && sidClaim.Value == sid)
            {
                Request.GetOwinContext().Authentication.SignOut("Cookies");
            }
        }
    }
}
