using Newtonsoft.Json;

namespace CaiqueServer.Firebase.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class ReceivedMessageAck
    {
        /// <summary>Required, string
        /// <para>This parameter specifies the recipient of a response message.</para>
        /// <para>The value must be a registration token of the client app that sent the upstream message.</para>
        /// </summary>
        [JsonProperty("to", Required = Required.Always)]
        public string To { get; set; }

        /// <summary>Required, string
        /// <para>This parameter specifies which message the response is intended for. The value must be the message_id value from the corresponding upstream message.</para>
        /// <para>The value must be a registration token of the client app that sent the upstream message.</para>
        /// </summary>
        [JsonProperty("message_id", Required = Required.Always)]
        public string MessageId { get; set; }

        /// <summary>Required, string
        /// <para>This parameter specifies an ack message from an app server to CCS. For upstream messages, it should always be set to ack.</para>
        /// </summary>
        [JsonProperty("message_type", Required = Required.Always)]
        public string MessageType { get; set; }
    }
}
