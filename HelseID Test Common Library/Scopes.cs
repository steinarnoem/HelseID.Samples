using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelseID.Test.WPF.Common
{
    public class Scopes : List<string>
    {
        public static string[] DefaultScopes = {"openid", "profile", "helseid://scopes/hpr", "helseid://scopes/identity/pid", "helseid://scopes/hpr/hpr_number", "helseid://scopes/identity/assurance_level", "helseid://scopes/identity/pid_pseudonym", "helseid://scopes/identity/security_level", "nhn/helseid.test.api.fullframework" };
    }
}
