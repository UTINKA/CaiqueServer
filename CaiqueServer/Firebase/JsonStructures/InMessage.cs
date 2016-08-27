using Newtonsoft.Json;

namespace CaiqueServer.Firebase.JsonStructures
{
    [JsonObject(MemberSerialization.OptIn)]
    class InMessage
    {
        [JsonProperty("from", Required = Required.Always)]
        public string From { get; set; }

        [JsonProperty("message_id", Required = Required.Always)]
        public string MessageId { get; set; }

        [JsonProperty("message_type", Required = Required.Always)]
        public string MessageType { get; set; }

        [JsonProperty("control_type", Required = Required.Default)]
        public string ControlType { get; set; }

        [JsonProperty("error", Required = Required.Default)]
        public string Error { get; set; }

        [JsonProperty("error_description", Required = Required.Default)]
        public string ErrorDesc { get; set; }

        [JsonProperty("category", Required = Required.Default)]
        public string Category { get; set; }

        [JsonObject(MemberSerialization.OptIn)]
        public class DataStructure
        {
            [JsonProperty("message_status", Required = Required.Always)]
            public string MessageStatus { get; set; }

            [JsonProperty("original_message_id", Required = Required.Always)]
            public string OrigMessageId { get; set; }

            [JsonProperty("device_registration_id", Required = Required.Always)]
            public string RegistrationId { get; set; }
        }

        [JsonProperty("data", Required = Required.Default)]
        public DataStructure Data { get; set; }
    }
}
