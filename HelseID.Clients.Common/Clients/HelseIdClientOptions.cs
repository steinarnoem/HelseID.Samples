using IdentityModel.OidcClient;
using static HelseID.Clients.Common.Jwt.JwtGenerator;

namespace HelseID.Clients.Common.Clients
{
    public class HelseIdClientOptions : OidcClientOptions
    {
        public string CertificateThumbprint { get; set; }

        public SigningMethod SigningMethod { get; set; }

        public string PreselectIdp { get; set; }
    }
}
