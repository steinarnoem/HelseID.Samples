using IdentityModel.Client;

namespace HelseID.Test.WPF.Common
{
    public class ClientAssertion
    {
        public static ClientAssertion CreateWithRsaKeys(string clientId, DiscoveryResponse discoDocument, JwtGenerator.SigningMethod signingMethod)
        {
            var assertion = JwtGenerator.Generate(clientId, discoDocument.TokenEndpoint, signingMethod);
            //JwtGenerator.ValidateToken(assertion, _options.ClientId);

            var clientAssertion = new ClientAssertion
            {
                Assertion = new { client_assertion = assertion, client_assertion_type = IdentityModel.OidcConstants.ClientAssertionTypes.JwtBearer }
            };
            return clientAssertion;
        }

        public object Assertion { get; set; }
    }
}
