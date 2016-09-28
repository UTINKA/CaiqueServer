using Newtonsoft.Json;

namespace CaiqueServer.Firebase.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class DatabaseUser
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name;
    }
}
