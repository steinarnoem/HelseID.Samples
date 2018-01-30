using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace HelseID.Test.WPF.Common
{
    public class JwtGenerator
    {
        public enum SigningMethod  {
            X509SecurityKey, RsaSecurityKey
        };

        public static List<string> ValidAudiences = new List<string> { "https://localhost:44366/connect/token", "https://helseid-sts.utvikling.nhn.no", "https://helseid-sts.test.nhn.no", "https://helseid-sts.utvikling.nhn.no"};
        private const double DefaultExpiryInHours = 10;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="tokenEndpoint"></param>
        /// <param name="signingMethod">Indicate which method to use when signing the Jwt Token</param>
        public static string Generate(string clientId, string tokenEndpoint, SigningMethod signingMethod)
        {
            return GenerateJwt(clientId, tokenEndpoint, null, signingMethod);
        }

        /// <summary>
        /// Generates a new JWT, uses Cng (RSA) to get PK SigningCredentials
        /// </summary>
        /// <param name="clientId">The OAuth/OIDC client ID</param>
        /// <param name="audience">The Authorization Server (STS)</param>
        /// <param name="expiryDate">If value is null, the default expiry date is used (10 hrs)</param>
        /// <param name="signingMethod"></param>
        /// <param name="signingCredentials">SigningCredentials for JWS, if this is null we get the credentials for existing CngKey or create a new PK pair.</param>
        /// <param name="x509Thumbprint"></param>
        /// <returns></returns>
        private static string GenerateJwt(string clientId, string audience, DateTime? expiryDate, SigningMethod signingMethod, SigningCredentials signingCredentials = null, string x509Thumbprint = null)
        {
            signingCredentials = GetSigningCredentials(signingMethod, signingCredentials, x509Thumbprint);
            
            var jwt = CreateJwtSecurityToken(clientId, audience + "", expiryDate, signingCredentials);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwt);
            return token;
        }

        private static JwtSecurityToken CreateJwtSecurityToken(string clientId, string audience, DateTime? expiryDate, SigningCredentials signingCredentials)
        {
            var exp = new DateTimeOffset(expiryDate ?? DateTime.Now.AddHours(DefaultExpiryInHours));

            var claims = new List<Claim>
            {
                new Claim("sub", clientId),
                new Claim("iat", exp.ToUnixTimeSeconds().ToString()),
                new Claim("jti", Guid.NewGuid().ToString("N"))
            };

            var token = new JwtSecurityToken(clientId, audience, claims, DateTime.Now, DateTime.Now.AddHours(10), signingCredentials);
            
            return token;
        }

        public static SecurityToken ValidateToken(string token, string validIssuer, string validAudience)
        {
            var publicKey = RSAKeyGenerator.GetPublicKeyAsXml();

            var test = RSA.Create();
            test.FromXmlString(publicKey);

            var securityKey = new RsaSecurityKey(test.ExportParameters(false));

            var handler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                IssuerSigningKey = securityKey,
                ValidAudience = validAudience,
                ValidIssuer = validIssuer
            };

            var claimsPrincipal = handler.ValidateToken(token, validationParams, out SecurityToken validatedToken);

            return validatedToken;
        }

        private static SigningCredentials GetSigningCredentials(SigningMethod signingMethod,
            SigningCredentials signingCredentials, string x509Thumbprint)
        {
            if (signingCredentials != null) return signingCredentials;

            switch (signingMethod)
            {
                case SigningMethod.RsaSecurityKey:
                    signingCredentials = GetRsaSigningCredentials();
                    break;
                case SigningMethod.X509SecurityKey:
                    if (x509Thumbprint.IsNotNullOrEmpty())
                        signingCredentials = GetX509SigningCredentials();
                    else
                        throw new ArgumentException(
                            "You must provide a certificate thumbprint to create signing credentials using X509 Certificates");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(signingMethod), signingMethod, null);
            }

            return signingCredentials;
        }

        private static SigningCredentials GetRsaSigningCredentials()
        {            
            var rsa = RSAKeyGenerator.GetRsaParameters();            
            var securityKey = new RsaSecurityKey(rsa);

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha512);
            
            return signingCredentials;
        }

        private static SigningCredentials GetX509SigningCredentials()
        {
            throw new NotImplementedException();
        }
    }
}
