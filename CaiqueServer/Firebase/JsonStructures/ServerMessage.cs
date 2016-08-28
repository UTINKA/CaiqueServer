using Newtonsoft.Json;

namespace CaiqueServer.Firebase.JsonStructures
{
    /// <summary>
    /// <para>This is a message sent from the XMPP connection server to an app server. Here are the primary types of messages that the XMPP connection server sends to the app server:</para>
    /// <para>Delivery Receipt: If the app server included delivery_receipt_requested in the downstream message, the XMPP connection server sends a delivery receipt when it receives confirmation that the device received the message.</para>
    /// <para>Control: These CCS-generated messages indicate that action is required from the app server.</para>
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    class ServerMessage
    {
        #region Common field
        /// <summary>Required, string
        /// <para>This parameter specifies the type of the CCS message: either delivery receipt or control.</para>
        /// <para>When it is set to receipt, the message includes from, message_id, category, and data fields to provide additional information.</para>
        /// <para>When it is set to control, the message includes control_type to indicate the type of control message.</para>
        /// </summary>
        [JsonProperty("message_type", Required = Required.Always)]
        public string MessageType { get; set; }
        #endregion

        #region Delivery receipt-specific
        /// <summary>Required, string
        /// <para>This parameter is set to gcm.googleapis.com, indicating that the message is sent from CCS.</para>
        /// </summary>
        [JsonProperty("from", Required = Required.Always)]
        public string From { get; set; }

        /// <summary>Required, string
        /// <para>This parameter specifies the original message ID that the receipt is intended for, prefixed with dr2: to indicate that the message is a delivery receipt. An app server must send an ack message with this message ID to acknowledge that it received this delivery receipt. See table 6 for the ack message format.</para>
        /// </summary>
        [JsonProperty("message_id", Required = Required.Always)]
        public string MessageId { get; set; }

        /// <summary>Optional, string
        /// <para>This parameter specifies the application package name of the client app that receives the message that this delivery receipt is reporting. This is available when message_type is receipt.</para>
        /// </summary>
        [JsonProperty("category", Required = Required.Default)]
        public string Category { get; set; }

        [JsonObject(MemberSerialization.OptIn)]
        public class DataPayload
        {
            /// <summary>This parameter specifies the status of the receipt message. It is set to MESSAGE_SENT_TO_DEVICE to indicate the device has confirmed its receipt of the original message.</summary>
            [JsonProperty("message_status", Required = Required.Always)]
            public string MessageStatus { get; set; }

            /// <summary>This parameter specifies the ID of the original message that the app server sent to the client app.</summary>
            [JsonProperty("original_message_id", Required = Required.Always)]
            public string OriginalMessageId { get; set; }

            /// <summary>This parameter specifies the registration token of the client app to which the original message was sent.</summary>
            [JsonProperty("device_registration_id", Required = Required.Always)]
            public string DeviceRegistrationId { get; set; }
        }

        /// <summary>Optional, string
        /// <para>This parameter specifies the key-value pairs for the delivery receipt message. This is available when the message type is receipt.</para>
        /// </summary>
        [JsonProperty("data", Required = Required.Default)]
        public DataPayload Data { get; set; }
        #endregion

        #region Control-specific
        /// <summary>Optional, string
        /// <para>This parameter specifies the type of control message sent from CCS.</para>
        /// <para>Currently, only CONNECTION_DRAINING is supported. The XMPP connection server sends this control message before it closes a connection to perform load balancing. As the connection drains, no more messages are allowed to be sent to the connection, but existing messages in the pipeline continue to be processed.</para>
        /// </summary>
        [JsonProperty("control_type", Required = Required.Default)]
        public string ControlType { get; set; }
        #endregion
    }
}