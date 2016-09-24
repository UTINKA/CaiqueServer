using Newtonsoft.Json;

namespace CaiqueServer.Firebase.JsonStructures
{
    [JsonObject(MemberSerialization.OptIn)]
    class User
    {
        [JsonProperty("mail", Required = Required.Always)]
        public string Mail;

        [JsonProperty("token", Required = Required.Always)]
        public string Token;

        [JsonProperty("name", Required = Required.Always)]
        public string Name;
    }
}
