using Newtonsoft.Json;

namespace CaiqueServer.Firebase.JsonStructures
{
    [JsonObject(MemberSerialization.OptIn)]
    class OutMessageResponse
    {
        [JsonProperty("from", Required = Required.Always)]
        public string From { get; set; }
        
        [JsonProperty("message_id", Required = Required.Always)]
        public string MessageId { get; set; }

        [JsonProperty("message_type", Required = Required.Always)]
        public string MessageType { get; set; }

        [JsonProperty("registration_id", Required = Required.Default)]
        public string RegistrationId { get; set; }

        [JsonProperty("error", Required = Required.Default)]
        public string Error { get; set; }

        [JsonProperty("error_description", Required = Required.Default)]
        public string ErrorDesc { get; set; }
    }
}
