using Newtonsoft.Json;
using System;

namespace CaiqueServer.Firebase.JsonStructures
{
    [JsonObject(MemberSerialization.OptIn)]
    class InMessageResponse
    {
        [JsonProperty("to", Required = Required.Always)]
        public string To { get; set; }
        
        [JsonProperty("message_id", Required = Required.Always)]
        public string MessageId { get; set; }

        [JsonProperty("message_type", Required = Required.Always)]
        public string MessageType { get; set; }
    }
}
