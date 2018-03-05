using Newtonsoft.Json;

namespace HelseID.Clients.HIDEnabler.Models
{
    public class Organization
    {
        [JsonProperty("Orgnr")]
        public string Nr { get; set; }

        [JsonProperty("Virksomhetsnavn")]
        public string Name { get; set; }
    }
}
