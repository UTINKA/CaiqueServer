using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaiqueServer.Firebase.JsonStructures
{
    [JsonObject(MemberSerialization.OptIn)]
    class UpstreamMessage
    {
        /// <summary>Required, string
        /// <para>This parameter specifies who sent this response.</para>
        /// <para>The value is the registration token of the client app.</para>
        /// </summary>
        [JsonProperty("from", Required = Required.Always)]
        public string From { get; set; }

        /// <summary>Required, string
        /// <para>This parameter specifies the application package name of the client app that receives the message that this delivery receipt is reporting.</para>
        /// </summary>
        [JsonProperty("category", Required = Required.Always)]
        public string Category { get; set; }

        /// <summary>Required, string
        /// <para>This parameter specifies the unique ID of the message.</para>
        /// </summary>
        [JsonProperty("message_id", Required = Required.Always)]
        public string MessageId { get; set; }

        /// <summary> 	Optional, string
        /// <para>This parameter specifies the key-value pairs of the message's payload</para>
        /// </summary>
        [JsonProperty("data", Required = Required.Default)]
        public JObject Data { get; set; }
    }
}
