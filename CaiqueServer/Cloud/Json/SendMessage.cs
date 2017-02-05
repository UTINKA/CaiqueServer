using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaiqueServer.Cloud.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class SendMessage
    {
        /// <summary>Optional, string
        /// <para>This parameter specifies the recipient of a message.</para>
        /// <para>The value must be a registration token, notification key, or topic. Do not set this field when sending to multiple topics. See condition.</para>
        /// </summary>
        [JsonProperty("to", Required = Required.Default)]
        public string To { get; set; }

        /// <summary>Optional, string
        /// <para>This parameter specifies a logical expression of conditions that determines the message target.</para>
        /// <para>Supported condition: Topic, formatted as "'yourTopic' in topics". This value is case-insensitive.</para>
        /// <para>Supported operators: &#38;&#38;, ||. Maximum two operators per topic message supported.</para>
        /// </summary>
        [JsonProperty("condition", Required = Required.Default)]
        public string Condition { get; set; }

        #region Options
        /// <summary>Required, string
        /// <para>This parameter uniquely identifies a message in an XMPP connection.</para>
        /// </summary>
        [JsonProperty("message_id", Required = Required.Always)]
        public string MessageId { get; set; }

        /// <summary>Optional, string
        /// <para>This parameter identifies a group of messages (e.g., with collapse_key: "Updates Available") that can be collapsed so that only the last message gets sent when delivery is resumed. This is intended to avoid sending too many of the same messages when the device comes back online or becomes active (see DelayIdle).</para>
        /// <para>There is no guarantee of the order in which messages get sent.</para>
        /// <para>Note: A maximum of 4 different collapse keys is allowed at any given time. This means an FCM connection server can simultaneously store 4 different send-to-sync messages per client app. If you exceed this number, there is no guarantee which 4 collapse keys the FCM connection server will keep. </para>
        /// </summary>
        [JsonProperty("collapse_key", Required = Required.Default)]
        public string CollapseKey { get; set; }

        /// <summary>Optional, string
        /// <para>Sets the priority of the message. Valid values are "normal" and "high." On iOS, these correspond to APNs priorities 5 and 10.</para>
        /// <para>By default, messages are sent with normal priority. Normal priority optimizes the client app's battery consumption and should be used unless immediate delivery is required. For messages with normal priority, the app may receive the message with unspecified delay.</para>
        /// <para>When a message is sent with high priority, it is sent immediately, and the app can wake a sleeping device and open a network connection to your server.</para>
        /// <para>For more information, see Setting the priority of a message.</para>
        /// </summary>
        [JsonProperty("priority", Required = Required.Default)]
        public string Priority { get; set; }

        /// <summary>Optional, JSON boolean
        /// <para>On iOS, use this field to represent content-available in the APNs payload. When a notification or message is sent and this is set to true, an inactive client app is awakened. On Android, data messages wake the app by default. On Chrome, this parameter is currently not supported.</para>
        /// </summary>
        [JsonProperty("content_available", Required = Required.Default)]
        public bool ContentAvailable { get; set; }

        /// <summary>Optional, JSON boolean
        /// <para>When this parameter is set to true, it indicates that the message should not be sent until the device becomes active.</para>
        /// <para>The default value is false.</para>
        /// </summary>
        [JsonProperty("delay_while_idle", Required = Required.Default)]
        public bool DelayIdle { get; set; }

        /// <summary>Optional, JSON number
        /// <para>This parameter specifies how long (in seconds) the message should be kept in FCM storage if the device is offline. The maximum time to live supported is 4 weeks, and the default value is 4 weeks. For more information, see Setting the lifespan of a message. </para>
        /// </summary>
        [JsonProperty("time_to_live", Required = Required.Default)]
        public string TTL { get; set; }

        /// <summary>Optional, JSON boolean
        /// <para>This parameter lets the app server request confirmation of message delivery.</para>
        /// <para>When this parameter is set to true, CCS sends a delivery receipt when the device confirms that it received the message.</para>
        /// <para>Note: this parameter is not supported for messages sent to iOS devices. The default value is false.</para>
        /// </summary>
        [JsonProperty("delivery_receipt_requested", Required = Required.Default)]
        public bool ReceiptRequested { get; set; }

        /// <summary>Optional, JSON boolean
        /// <para>This parameter, when set to true, allows developers to test a request without actually sending a message.</para>
        /// <para>The default value is false.</para>
        /// </summary>
        [JsonProperty("dry_run", Required = Required.Default)]
        public bool DryRun { get; set; }
        #endregion

        #region Payload
        /// <summary>Optional, JSON object
        /// <para>This parameter specifies the key-value pairs of the message's payload.</para>
        /// <para>For example, with data:{"score":"3x1"}:</para>
        /// <para>On iOS, if the message is sent via APNs, it represents the custom data fields. If it is sent via the FCM connection server, it is represented as a key value dictionary in AppDelegate application:didReceiveRemoteNotification:.</para>
        /// <para>On Android, this results in an intent extra named score with the string value 3x1.</para>
        /// <para>The key should not be a reserved word ("from" or any word starting with "google" or "gcm"). Do not use any of the words defined in this table (such as collapse_key).</para>
        /// <para>Values in string types are recommended. You have to convert values in objects or other non-string data types (e.g., integers or booleans) to string.</para>
        /// </summary>
        [JsonProperty("data", Required = Required.Default)]
        //public JObject Data { get; set; }
        public object Data { get; set; }

        [JsonObject(MemberSerialization.OptIn)]
        public class NotificationPayload
        {
            /// <summary>Optional, string
            /// <para>Indicates notification title.</para>
            /// </summary>
            [JsonProperty("title", Required = Required.Default)]
            public string Title { get; set; }

            /// <summary>Optional, string
            /// <para>Indicates notification body text. </para>
            /// </summary>
            [JsonProperty("text", Required = Required.Default)]
            public string Text { get; set; }

            /// <summary>Optional, string
            /// <para>Indicates notification icon. Sets value to myicon for drawable resource myicon.</para>
            /// </summary>
            [JsonProperty("icon", Required = Required.Default)]
            public string Icon { get; set; }

            /// <summary>Optional, string
            /// <para>Indicates a sound to play when the device receives a notification. Supports default or the filename of a sound resource bundled in the app. Sound files must reside in /res/raw/.</para>
            /// </summary>
            [JsonProperty("sound", Required = Required.Default)]
            public string Sound { get; set; }

            /// <summary>Optional, string
            /// <para>Indicates whether each notification results in a new entry in the notification drawer on Android. </para>
            /// <para>If not set, each request creates a new notification.</para>
            /// <para>If set, and a notification with the same tag is already being shown, the new notification replaces the existing one in the notification drawer.</para>
            /// </summary>
            [JsonProperty("tag", Required = Required.Default)]
            public string Tag { get; set; }

            /// <summary>Optional, string
            /// <para>Indicates color of the icon, expressed in #rrggbb format</para>
            /// </summary>
            [JsonProperty("color", Required = Required.Default)]
            public string Color { get; set; }

            /// <summary>Optional, string
            /// <para>Indicates the action associated with a user click on the notification. When this is set, an activity with a matching intent filter is launched when user clicks the notification.</para>
            /// </summary>
            [JsonProperty("click_action", Required = Required.Default)]
            public string ClickAction { get; set; }

            /// <summary>Optional, string
            /// <para>Indicates the key to the body string for localization. Use the key in the app's string resources when populating this value.</para>
            /// </summary>
            [JsonProperty("body_loc_key", Required = Required.Default)]
            public string BodyLocKey { get; set; }

            /// <summary>Optional, JSON array as string
            /// <para>Indicates the string value to replace format specifiers in the body string for localization. For more information, see Formatting and Styling.</para>
            /// </summary>
            [JsonProperty("body_loc_args", Required = Required.Default)]
            public JArray BodyLocArgs { get; set; }

            /// <summary>Optional, string
            /// <para>Indicates the key to the title string for localization. Use the key in the app's string resources when populating this value.</para>
            /// </summary>
            [JsonProperty("title_loc_key", Required = Required.Default)]
            public string TitleLocKey { get; set; }

            /// <summary>Optional, JSON array as string
            /// <para>Indicates the string value to replace format specifiers in the title string for localization. For more information, see Formatting strings.</para>
            /// </summary>
            [JsonProperty("title_loc_args", Required = Required.Default)]
            public JArray TitleLocArgs { get; set; }
        }

        /// <summary>Optional, JSON object
        /// <para>This parameter specifies the key-value pairs of the notification payload. See Notification payload support for detail. For more information about notification message and data message options, see message types.</para>
        /// </summary>
        [JsonProperty("notification", Required = Required.Default)]
        public NotificationPayload Notification { get; set; }
        #endregion
    }
}
