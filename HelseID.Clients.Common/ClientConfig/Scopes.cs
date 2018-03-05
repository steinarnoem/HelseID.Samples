using System.Collections.Generic;

namespace HelseID.Clients.Common.ClientConfig
{
    public class Scopes : List<string>
    {
        public static string[] DefaultScopes = {"openid", "profile", "offline_access", "helseid://scopes/hpr", "helseid://scopes/identity/pid", "helseid://scopes/hpr/hpr_number", "helseid://scopes/identity/assurance_level", "helseid://scopes/identity/pid_pseudonym", "helseid://scopes/identity/security_level", "nhn/helseid.test.api.fullframework" };
    }
}
