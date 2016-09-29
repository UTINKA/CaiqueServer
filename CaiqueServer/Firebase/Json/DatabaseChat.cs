using Newtonsoft.Json;

namespace CaiqueServer.Firebase.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class DatabaseChat
    {
        [JsonProperty("title", Required = Required.Always)]
        public string Title;

        [JsonProperty("picture", Required = Required.Always)]
        public string Picture;

        [JsonProperty("tags", Required = Required.Always)]
        public string[] Tags;
    }
}
