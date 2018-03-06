using HelseID.Common.ClientConfig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace HelseID.Clients.WPF.Controls
{
    public class Setting
    {
        public string Name { get; set; }

        public string Authority { get; set; }

        public string ClientId { get; set; }

        public List<string> Scopes { get; set; } = new List<string>();

        public string ClientSecret { get; set; }

        public string RedirectUrl { get; set; }
        public string PreselectedIdp { get; set; }
    }

    public class Config
    {
        public static List<Setting> Settings()
        {
            try
            {
                using (var r = new StreamReader(Directory.GetCurrentDirectory() + "/appsettings.json"))
                {
                    string json = r.ReadToEnd();
                    List<Setting> items = JsonConvert.DeserializeObject<List<Setting>>(json);
                    return items;
                }
            }
            catch (Exception)
            {
                return new List<Setting>
                {
                    new Setting
                    {
                        Name = "Default local",
                        ClientId = DefaultClientConfigurationValues.DefaultClientId,
                        Scopes = new List<string>{DefaultClientConfigurationValues.DefaultScope },
                        ClientSecret = DefaultClientConfigurationValues.DefaultSecret,
                        RedirectUrl = "http://127.0.0.1:4444/",
                        Authority = DefaultClientConfigurationValues.DefaultAuthority,
                        PreselectedIdp = ""
                    }
                };
            }
        }

        public static void Save(List<Setting> settings)
        {
            using (var r = new StreamWriter(Directory.GetCurrentDirectory() + "/appsettings.json"))
            {
                var json = JsonConvert.SerializeObject(settings);
                r.Write(json);
            }
        }
    }
}
