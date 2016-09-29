using Newtonsoft.Json;

namespace CaiqueServer.Firebase.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class DatabaseUser
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name;

        [JsonProperty("picture", Required = Required.Default)]
        public string Picture;
    }
}
