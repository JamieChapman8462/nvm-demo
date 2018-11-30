using System.Collections.Generic;
using Newtonsoft.Json;

namespace InteractionPlanApi.Request.NCCO
{
    public class Connect : NCCO
    {
        [JsonProperty("action")]
        public string Action => "connect";

        [JsonProperty("endpoint")]
        public EndPoint EndPoint { get; set; }
    }

    public class EndPoint
    {
        [JsonProperty("type")]
        public string Type => "phone";

        [JsonProperty("number")]
        public string  Number { get; set; }
    }

    public class Talk : NCCO
    {
        [JsonProperty("action")]
        public string Action => "talk";

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class AudioStream : NCCO
    {
        [JsonProperty("action")]
        public string Action => "stream";

        [JsonProperty("streamUrl")]
        public ICollection<string> StreamUrl { get; set; }
    }
}