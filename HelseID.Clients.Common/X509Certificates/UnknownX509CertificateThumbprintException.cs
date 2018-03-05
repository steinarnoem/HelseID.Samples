using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelseID.Clients.Common.X509Certificates
{
    public class UnknownX509CertificateThumbprintException : Exception
    {
        public UnknownX509CertificateThumbprintException(string message) : base(message)
        {
        }
    }
}
