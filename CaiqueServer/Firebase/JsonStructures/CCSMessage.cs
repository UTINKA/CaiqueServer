using Newtonsoft.Json;

namespace CaiqueServer.Firebase.JsonStructures
{
    [JsonObject(MemberSerialization.OptIn)]
    class CCSMessage
    {
        [JsonProperty("message_type", Required = Required.Always)]
        public string MessageType { get; set; }

        [JsonProperty("from", Required = Required.Always)]
        public string From { get; set; }

        [JsonProperty("message_id", Required = Required.Always)]
        public string MessageId { get; set; }

        [JsonProperty("category", Required = Required.Default)]
        public string Category { get; set; }

        [JsonProperty("data", Required = Required.Default)]
        public string Data { get; set; }

        [JsonProperty("control_type", Required = Required.Default)]
        public string ControlType { get; set; }
    }
}
