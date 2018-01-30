using IdentityModel.Client;

namespace HelseID.Test.WPF.Common
{
    public class ClientAssertion
    {
        public static object CreateWithRsaKeys(string clientId, DiscoveryResponse discoDocument, JwtGenerator.SigningMethod signingMethod)
        {
            var assertion = JwtGenerator.Generate(clientId, discoDocument.TokenEndpoint, signingMethod);
            //JwtGenerator.ValidateToken(assertion, _options.ClientId);

            var clientAssertion = new ClientAssertion
            {
                //Assertion = assertion //new { client_assertion = assertion, client_assertion_type = IdentityModel.OidcConstants.ClientAssertionTypes.JwtBearer }
            };
            return new { client_assertion=assertion,client_assertion_type=IdentityModel.OidcConstants.ClientAssertionTypes.JwtBearer };
        }

        public string Assertion { get; set; }
        public string AssertionType = IdentityModel.OidcConstants.ClientAssertionTypes.JwtBearer;
    }
}
