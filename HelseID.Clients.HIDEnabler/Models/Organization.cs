using Newtonsoft.Json;

namespace HelseID.Clients.HIDEnabler.Models
{
    public class Organization
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("organization_number")]
        public string Nr { get; set; }
    }
}
