using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace HelseID.Clients.Common.Extensions
{
    public static class JwtSecurityTokenExtensions
    {
        public static string SerializeToken(this JwtSecurityToken jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwt);            
            return token;
        }

        public static void UpdateJwtHeader(this JwtSecurityToken token, SecurityKey key)
        {
            if (!(key is X509SecurityKey))
                return;

            var x509Key = (X509SecurityKey)key;

            var thumbprint = Base64Url.Encode(x509Key.Certificate.GetCertHash());
            var x5C = GenerateX5C(x509Key.Certificate);
            var pubKey = x509Key.PublicKey as RSA;
            var parameters = pubKey.ExportParameters(false);
            var exponent = Base64Url.Encode(parameters.Exponent);
            var modulus = Base64Url.Encode(parameters.Modulus);

            token.Header.Add("x5c", x5C);
            token.Header.Add("kty", pubKey.SignatureAlgorithm);
            token.Header.Add("use", "sig");
            token.Header.Add("x5t", thumbprint);
            token.Header.Add("e", exponent);
            token.Header.Add("n", modulus);
        }

        private static List<string> GenerateX5C(X509Certificate2 certificate)
        {
            var x5C = new List<string>();
            var chain = GetCertificateChain(certificate);

            if (chain == null) return x5C;

            foreach (var cert in chain.ChainElements)
            {
                var x509Base64 = Convert.ToBase64String(cert.Certificate.RawData);
                x5C.Add(x509Base64);
            }

            return x5C;
        }

        private static X509Chain GetCertificateChain(X509Certificate2 cert)
        {
            var certificateChain = X509Chain.Create();
            certificateChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            certificateChain.Build(cert);
            return certificateChain;
        }
    }
}
