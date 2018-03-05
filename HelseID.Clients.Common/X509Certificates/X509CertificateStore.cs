using System;
using System.Security.Cryptography.X509Certificates;
using HelseID.Clients.Common.Extensions;

namespace HelseID.Clients.Common.X509Certificates
{
    public class X509CertificateStore
    {
        public static X509Certificate2 GetX509CertificateByThumbprint(string thumbprint, StoreName store = StoreName.My, StoreLocation location = StoreLocation.CurrentUser)
        {
            if (thumbprint.IsNullOrEmpty())
                throw new ArgumentOutOfRangeException(nameof(thumbprint));

            using (var x509Store = new X509Store(store, location))
            {
                x509Store.Open(OpenFlags.ReadOnly);

                var certificatesInStore = x509Store.Certificates;
                var certificates = certificatesInStore.Find(X509FindType.FindByThumbprint, thumbprint, false);

                if (certificates.Count < 1)
                    throw new UnknownX509CertificateThumbprintException($"Did not find any Certificates with the thumbprint: {thumbprint}");

                if (certificates.Count > 1)
                    throw new Exception($"Found {certificates.Count} certificates with thumbprint: {thumbprint}");

                var certificate = certificates[0];

                return certificate;
            }
        }
    }
}
