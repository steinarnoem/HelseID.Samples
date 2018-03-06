using System;

namespace HelseID.Common.Certificates
{
    public class UnknownCertificateThumbprintException : Exception
    {
        public UnknownCertificateThumbprintException(string message) : base(message)
        {
        }
    }
}
