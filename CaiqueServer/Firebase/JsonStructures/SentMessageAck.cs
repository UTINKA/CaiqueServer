using Newtonsoft.Json;

namespace CaiqueServer.Firebase.JsonStructures
{
    [JsonObject(MemberSerialization.OptIn)]
    class SentMessageAck
    {
        /// <summary>Required, string
        /// <para>This parameter specifies who sent this response.</para>
        /// <para>The value is the registration token of the client app.</para>
        /// </summary>
        [JsonProperty("from", Required = Required.Always)]
        public string From { get; set; }

        /// <summary>Required, string
        /// <para>This parameter uniquely identifies a message in an XMPP connection. The value is a string that uniquely identifies the associated message.</para>
        /// </summary>
        [JsonProperty("message_id", Required = Required.Always)]
        public string MessageId { get; set; }

        /// <summary>Required, string
        /// <para>This parameter specifies an ack or nack message from the XMPP connection server to the app server.</para>
        /// <para>If the value is set to nack, the app server should look at error and error_description to get failure information.</para>
        /// </summary>
        [JsonProperty("message_type", Required = Required.Always)]
        public string MessageType { get; set; }
        
        /// <summary>Optional, string
        /// <para>This parameter specifies the canonical registration token for the client app that the message was processed and sent to. Sender should replace the registration token with this value on future requests; otherwise, the messages might be rejected.</para>
        /// </summary>
        [JsonProperty("registration_id", Required = Required.Default)]
        public string RegistrationId { get; set; }

        /// <summary>Optional, string
        /// <para>This parameter specifies an error related to the downstream message. It is set when the message_type is nack. See table 4 for details.</para>
        /// </summary>
        [JsonProperty("error", Required = Required.Default)]
        public string Error { get; set; }

        /// <summary>Optional, string
        /// <para>This parameter provides descriptive information for the error. It is set when the message_type is nack.</para>
        /// </summary>
        [JsonProperty("error_description", Required = Required.Default)]
        public string ErrorDesc { get; set; }
    }
}
