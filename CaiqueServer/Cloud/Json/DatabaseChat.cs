using Newtonsoft.Json;

namespace CaiqueServer.Cloud.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class DatabaseChat
    {
        [JsonProperty("title", Required = Required.Always)]
        public string Title;

        [JsonProperty("tags", Required = Required.Default)]
        public string[] Tags;
    }
}
