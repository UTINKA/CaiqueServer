using Newtonsoft.Json;

namespace CaiqueServer.Firebase.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class DatabaseToken
    {
        [JsonProperty("id", Required = Required.Always)]
        public long Id;
    }
}
