using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelseID.Test.WPF.Common
{
    public class DefaultClientConfigurationValues
    {
        public const string DefaultAuthority = "https://helseid-sts.utvikling.nhn.no/";
        public const string DefaultClientId = "wpfdemo";
        public const string DefaultScope = "openid";
        public const string DefaultSecret = "AYzJzIsQl2pqPmSEsfJG7ieJ5FCTjheHkvxphm8Axsk91hzRNE0KfI22bfXsDHbi";
        public const string DefaultUri = "http://127.0.0.1:7890/";  //"https://localhost/wpfclient-callback";
    }
}
