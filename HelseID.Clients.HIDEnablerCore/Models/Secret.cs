﻿using Newtonsoft.Json;

namespace HelseID.Clients.HIDEnabler.Services
{
    public class Secret
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("expiration")]
        public string Expiration { get; set; }
    }
}
