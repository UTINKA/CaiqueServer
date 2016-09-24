﻿using Newtonsoft.Json;

namespace CaiqueServer.Firebase.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class DatabaseUser
    {
        [JsonProperty("mail", Required = Required.Always)]
        public string Mail;

        [JsonProperty("token", Required = Required.Always)]
        public string Token;

        [JsonProperty("name", Required = Required.Always)]
        public string Name;
    }
}