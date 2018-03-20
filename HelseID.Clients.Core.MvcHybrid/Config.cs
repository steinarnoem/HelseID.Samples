using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using HelseID.Common.Extensions;

namespace HelseID.Clients.Core.MvcHybrid
{
    public class Config
    {
        public static Settings Setting()
        {
            try
            {
                using (var r = new StreamReader(Directory.GetCurrentDirectory() + "/settings.json"))
                {
                    string json = r.ReadToEnd();
                    var setting = JsonConvert.DeserializeObject<Settings>(json);
                    return setting;
                }
            }
            catch (Exception)
            {
                return
                    new Settings
                    {
                        Authority = "https://helseid-sts.utvikling.nhn.no",
                        RedirectUri = "http://localhost:21402/signin-oidc",
                        PostLogoutRedirectUri = "http://localhost:21402/signout-oidc",
                        SigningMethod = "3",
                        CertificateThumbprint = "d7e313c62fdd723d6e306e43e22c98ac5fb91d47",
                        ClientId = "ef5dc5a8-52dc-4454-b162-6f82e647978a",
                        ResponseType = "code id_token",
                        Scope = new List<string> {
                            "openid",
                            "profile",
                            "offline_access",
                            "helseid://scopes/identity/pid",
                            "helseid://scopes/identity/pid_pseudonym",
                            "helseid://scopes/identity/assurance_level",
                            "helseid://scopes/identity/security_level",
                            "nhn/helseid.test.api.fullframework" }.ToSpaceSeparatedList()
                    };
            }
        }

        public static void Save(Settings settings)
        {
            using (var r = new StreamWriter(Directory.GetCurrentDirectory() + "/settings.json"))
            {
                var json = JsonConvert.SerializeObject(settings);
                r.Write(json);
            }
        }
    }
}
