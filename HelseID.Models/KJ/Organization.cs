using Newtonsoft.Json;

namespace HelseID.Models.KJ
{
    public class Organization
    {
        [JsonProperty("Orgnr")]
        public string Nr { get; set; }

        [JsonProperty("Virksomhetsnavn")]
        public string Name { get; set; }
    }
}
