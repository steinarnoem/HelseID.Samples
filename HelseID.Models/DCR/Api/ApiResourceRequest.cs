using Newtonsoft.Json;

namespace HelseID.Models.DCR.Api
{
    public class ApiResourceRequest : ApiResource
    {
        [JsonProperty("secrets")]
        public Secret[] Secrets { get; set; }
    }
}