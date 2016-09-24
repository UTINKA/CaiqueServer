using Newtonsoft.Json;

namespace CaiqueServer.Firebase.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class DatabaseMessage
    {
        [JsonProperty("chat", Required = Required.Always)]
        public int Chat;

        [JsonProperty("sender", Required = Required.Always)]
        public long Sender;
        
        [JsonProperty("type", Required = Required.Always)]
        public string Type;

        [JsonProperty("date", Required = Required.Always)]
        public int Date;

        [JsonProperty("text", Required = Required.Default)]
        public string Text;

        [JsonProperty("attach", Required = Required.Default)]
        public long Attachment;
    }
}
