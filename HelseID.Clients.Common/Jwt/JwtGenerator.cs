using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using HelseID.Clients.Common.Crypto;
using HelseID.Clients.Common.Extensions;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace HelseID.Clients.Common.Jwt
{
    public class JwtGenerator
    {
        public enum SigningMethod
        {
            None, X509SecurityKey, RsaSecurityKey, X509EnterpriseSecurityKey
        };

        public static List<string> ValidAudiences = new List<string> { "https://localhost:44366/connect/token", "https://helseid-sts.utvikling.nhn.no", "https://helseid-sts.test.nhn.no", "https://helseid-sts.utvikling.nhn.no" };
        private const double DefaultExpiryInHours = 10;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="tokenEndpoint"></param>
        /// <param name="signingMethod">Indicate which method to use when signing the Jwt Token</param>
        /// <param name="securityKey"></param>
        /// <param name="securityAlgorithm"></param>
        public static string Generate(string clientId, string tokenEndpoint, SigningMethod signingMethod, SecurityKey securityKey, string securityAlgorithm)
        {
            if (clientId.IsNullOrEmpty())
                throw new ArgumentException("clientId can not be empty or null");

            if (tokenEndpoint.IsNullOrEmpty())
                throw new ArgumentException("The token endpoint address can not be empty or null");

            if (securityKey ==null)
                throw new ArgumentException("The security key can not be null");

            if (securityAlgorithm.IsNullOrEmpty())
                throw new ArgumentException("The security algorithm can not be empty or null");

            return GenerateJwt(clientId, tokenEndpoint, null, signingMethod, securityKey, securityAlgorithm);
        }

        /// <summary>
        /// Generates a new JWT
        /// </summary>
        /// <param name="clientId">The OAuth/OIDC client ID</param>
        /// <param name="audience">The Authorization Server (STS)</param>
        /// <param name="expiryDate">If value is null, the default expiry date is used (10 hrs)</param>
        /// <param name="signingMethod"></param>
        /// <param name="securityKey"></param>
        /// <param name="securityAlgorithm"></param>
        /// <returns></returns>
        private static string GenerateJwt(string clientId, string audience, DateTime? expiryDate, SigningMethod signingMethod, SecurityKey securityKey, string securityAlgorithm) //SigningMethod signingMethod, SigningCredentials signingCredentials = null)
        {            
            var signingCredentials = new SigningCredentials(securityKey, securityAlgorithm);

            var jwt = CreateJwtSecurityToken(clientId, audience + "", expiryDate, signingCredentials);

            if (signingMethod == SigningMethod.X509EnterpriseSecurityKey)
                UpdateJwtHeader(securityKey, jwt);


            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(jwt);
        }

        public static void UpdateJwtHeader(SecurityKey key, JwtSecurityToken token)
        {
            if (key is X509SecurityKey x509Key)
            {
                var thumbprint = Base64Url.Encode(x509Key.Certificate.GetCertHash());
                var x5c = GenerateX5c(x509Key.Certificate);
                var pubKey = x509Key.PublicKey as RSA;
                var parameters = pubKey.ExportParameters(false);
                var exponent = Base64Url.Encode(parameters.Exponent);
                var modulus = Base64Url.Encode(parameters.Modulus);

                token.Header.Add("x5c", x5c);
                token.Header.Add("kty", pubKey.SignatureAlgorithm);
                token.Header.Add("use", "sig");
                token.Header.Add("x5t", thumbprint);
                token.Header.Add("e", exponent);
                token.Header.Add("n", modulus);
            }

            if (key is RsaSecurityKey rsaKey)
            {
                var parameters = rsaKey.Rsa?.ExportParameters(false) ?? rsaKey.Parameters;
                var exponent = Base64Url.Encode(parameters.Exponent);
                var modulus = Base64Url.Encode(parameters.Modulus);

                token.Header.Add("kty", "RSA");
                token.Header.Add("use", "sig");
                token.Header.Add("e", exponent);
                token.Header.Add("n", modulus);
            }
        }

        private static List<string> GenerateX5c(X509Certificate2 certificate)
        {
            var x5c = new List<string>();
            var chain = GetCertificateChain(certificate);
            if (chain != null)
            {
                foreach (var cert in chain.ChainElements)
                {
                    var x509base64 = Convert.ToBase64String(cert.Certificate.RawData);
                    x5c.Add(x509base64);
                }
            }
            return x5c;
        }

        private static X509Chain GetCertificateChain(X509Certificate2 cert)
        {
            X509Chain certificateChain = X509Chain.Create();
            certificateChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            certificateChain.Build(cert);
            return certificateChain;
        }

        private static JwtSecurityToken CreateJwtSecurityToken(string clientId, string audience, DateTime? expiryDate, SigningCredentials signingCredentials)
        {
            var exp = new DateTimeOffset(expiryDate ?? DateTime.Now.AddHours(DefaultExpiryInHours));

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, clientId),
                new Claim(JwtClaimTypes.IssuedAt, exp.ToUnixTimeSeconds().ToString()),
                new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N"))
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

            var claimsPrincipal = handler.ValidateToken(token, validationParams, out var validatedToken);

            return validatedToken;
        }        

    }
}
