using IdentityModel.Client;

namespace HelseID.Test.WPF.Common
{
    public class ClientAssertion
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId">The OIDC/OAuth client id</param>
        /// <param name="tokenEndpointUrl">The adress to the token endpoint of the STS (AS)</param>
        /// <param name="signingMethod">Indicate which method you would like to use to sign the token</param>
        /// <returns>Client assertion (JWT) and client assertion type</returns>
        public static object CreateWithRsaKeys(string clientId, string tokenEndpointUrl, JwtGenerator.SigningMethod signingMethod)
        {
            var assertion = JwtGenerator.Generate(clientId, tokenEndpointUrl, signingMethod);
            //JwtGenerator.ValidateToken(assertion, _options.ClientId);

            var clientAssertion = new ClientAssertion
            {
                Assertion = assertion 
            };

            return new { client_assertion = clientAssertion.Assertion,client_assertion_type = clientAssertion.AssertionType}; ;
        }

        public string Assertion { get; set; }
        public string AssertionType = IdentityModel.OidcConstants.ClientAssertionTypes.JwtBearer;
    }
}
