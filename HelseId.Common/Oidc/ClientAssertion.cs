﻿using HelseID.Common.Certificates;
using HelseID.Common.Crypto;
using HelseID.Common.Jwt;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace HelseID.Common.Oidc
{
    public class ClientAssertion
    {
        public static ClientAssertion CreateWithRsaKeys(string clientId, string tokenEndpointUrl)
        {
            var rsa = RSAKeyGenerator.GetRsaParameters();
            var securityKey = new RsaSecurityKey(rsa);
            var assertion = JwtGenerator.Generate(clientId, tokenEndpointUrl, JwtGenerator.SigningMethod.RsaSecurityKey, securityKey, SecurityAlgorithms.RsaSha512);

            return new ClientAssertion{ client_assertion = assertion };
        }

        public static ClientAssertion CreateWithEnterpriseCertificate(string clientId, string tokenEndpointUrl, string thumbprint)
        {
            var certificate = CertificateStore.GetCertificateByThumbprint(thumbprint);
            var securityKey = new X509SecurityKey(certificate);
            var assertion = JwtGenerator.Generate(clientId, tokenEndpointUrl, JwtGenerator.SigningMethod.X509EnterpriseSecurityKey, securityKey, SecurityAlgorithms.RsaSha512);

            return new ClientAssertion { client_assertion = assertion };
        }

        [JsonProperty("client_assertion")]
        public string client_assertion { get; set; }

        [JsonProperty("client_assertion_type")]
        public string client_assertion_type { get; set; } = IdentityModel.OidcConstants.ClientAssertionTypes.JwtBearer;
    }
}
