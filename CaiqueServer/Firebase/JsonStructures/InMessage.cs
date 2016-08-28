using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaiqueServer.Firebase.JsonStructures
{
    [JsonObject(MemberSerialization.OptIn)]
    class InMessage
    {
        [JsonProperty("from", Required = Required.Always)]
        public string From { get; set; }
        
        [JsonProperty("category", Required = Required.Always)]
        public string Category { get; set; }

        [JsonProperty("message_id", Required = Required.Always)]
        public string MessageId { get; set; }

        [JsonProperty("data", Required = Required.Default)]
        public JObject Data { get; set; }

        public InMessageResponse GetAck()
        {
            return new InMessageResponse
            {
                To = From,
                MessageId = MessageId,
                MessageType = "ack"
            };
        }
    }
}
