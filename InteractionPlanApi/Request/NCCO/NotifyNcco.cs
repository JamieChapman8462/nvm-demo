using Newtonsoft.Json;

namespace InteractionPlanApi.Request.NCCO
{
    public class Notify : NCCO
    {
        [JsonProperty("action")]
        public string Action => "notify";

        [JsonProperty("payload")]
        public Payload Payload { get; set; }
    }

    public class Payload
    {
        [JsonProperty("data")]
        public string Data { get; set; }
    }
}