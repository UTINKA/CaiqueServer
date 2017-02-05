using Newtonsoft.Json;

namespace CaiqueServer.Cloud.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class Event
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type;

        [JsonProperty("chat", Required = Required.Default)]
        public string Chat;

        [JsonProperty("sender", Required = Required.Default)]
        public string Sender;

        [JsonProperty("date", Required = Required.Default)]
        public int Date;

        [JsonProperty("text", Required = Required.Default)]
        public string Text;

        [JsonProperty("attach", Required = Required.Default)]
        public string Attachment;
    }
}
