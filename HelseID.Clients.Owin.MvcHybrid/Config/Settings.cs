using HelseID.Common.Jwt;
using HelseID.Common.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;
using HelseID.Common.Crypto;

namespace HelseID.Clients.Owin.MvcHybrid
{
    public class Settings
    {
        public string ClientId { get; set; }
        public string Authority { get; set; }
        public string RedirectUri { get; set; }
        public string PostLogoutRedirectUri { get; set; }
        public string ResponseType { get; set; }
        public string Scope { get; set; }
        public string SigningMethod { get; set; }
        public string CertificateThumbprint { get; set; }

        public string RsaKey { get { return RSAKeyGenerator.GetPublicKeyAsXml(); } }
        public string ClientSecret { get; set; }

        public string IdentityProvider { get; set; }
        public bool OnlyLevel4 { get; set; }
        public string Scopes {
            get {
                return string.Join("\n", Scope.FromSpaceSeparatedToList());
            }
            set {
                Scope = value.Replace("\r", "").Split(new string[] { "\n" }, StringSplitOptions.None).ToSpaceSeparatedList();
            }
        }

        [MaxLength(11)]
        [MinLength(11)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Fødselnummer må være et tall med 11 siffer")]
        public string TestPid { get; set; }

        public string TestHprNumber { get; set; }

        public string TestSecurityLevel { get; set; }

        public bool ForceLogin { get; set; }

        public List<SelectListItem> SecurityLevels
        {
            get
            {
                var list = new List<SelectListItem>
                {
                    new SelectListItem {Value="2", Text="Level 2"},
                    new SelectListItem {Value="3", Text="Level 3"},
                    new SelectListItem {Value="4", Text="Level 4"},
                };
                return list;
            }
        }

        public List<SelectListItem> IdentityProviders
        {
            get
            {
                var list = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Ingen" },
                    new SelectListItem { Value = "idporten-oidc", Text = "ID-porten" },
                    new SelectListItem { Value = "testidp-oidc", Text = "Test IDP" }
                };

                return list;
            }
        }

        public List<SelectListItem> Authorities
        {
            get
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Value = "https://localhost:46611", Text = "Localhost" },
                    new SelectListItem { Value = "https://helseid-sts.utvikling.nhn.no", Text = "Utvikling" },
                    new SelectListItem { Value = "https://helseid-sts.test.nhn.no", Text = "Test" },
                    new SelectListItem { Value = "https://helseid-sts.nhn.no", Text = "Prod" }
                };
            }
        }

        public List<SelectListItem> SigningMethods
        {
            get
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Value = ((int)JwtGenerator.SigningMethod.None).ToString(), Text = "None" },
                    new SelectListItem { Value = ((int)JwtGenerator.SigningMethod.RsaSecurityKey).ToString(), Text = "RsaSecurityKey" },
                    new SelectListItem { Value = ((int)JwtGenerator.SigningMethod.X509SecurityKey).ToString(), Text = "X509SecurityKey" },
                    new SelectListItem { Value = ((int)JwtGenerator.SigningMethod.X509EnterpriseSecurityKey).ToString(), Text = "X509EnterpriseSecurityKey" }
                };
            }
        }


    }
}
