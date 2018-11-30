using Newtonsoft.Json;

namespace InteractionPlanApi.Request.NCCO
{
    public interface NCCO
    {
    }

    public class Conversation : NCCO
    {
        [JsonProperty("action")]
        public string Action => "conversation";

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}