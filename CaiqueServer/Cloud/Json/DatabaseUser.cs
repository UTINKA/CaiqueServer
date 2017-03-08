using Newtonsoft.Json;

namespace CaiqueServer.Cloud.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class DatabaseUser
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name;
    }
}
