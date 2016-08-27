using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaiqueServer.Firebase.JsonStructures
{
    [JsonObject(MemberSerialization.OptIn)]
    class OutMessage
    {
        [JsonProperty("to", Required = Required.Default)]
        public string To { get; set; }

        [JsonProperty("condition", Required = Required.Default)]
        public string Condition { get; set; }

        [JsonProperty("message_id", Required = Required.Always)]
        public string MessageId { get; set; }

        [JsonProperty("collapse_key", Required = Required.Default)]
        public string CollapseKey { get; set; }

        [JsonProperty("priority", Required = Required.Default)]
        public string Priority { get; set; }

        [JsonProperty("content_available", Required = Required.Default)]
        public bool ContentAvailable { get; set; }

        [JsonProperty("delay_while_idle", Required = Required.Default)]
        public bool DelayIdle { get; set; }

        [JsonProperty("time_to_live", Required = Required.Default)]
        public string TTL { get; set; }

        [JsonProperty("delivery_receipt_requested", Required = Required.Default)]
        public bool ReceiptRequested { get; set; }

        [JsonProperty("dry_run", Required = Required.Default)]
        public bool DryRun { get; set; }

        [JsonProperty("data", Required = Required.Default)]
        public JObject Data { get; set; }

        [JsonObject(MemberSerialization.OptIn)]
        public class NotificationPayload
        {
            [JsonProperty("title", Required = Required.Default)]
            public string Title { get; set; }

            [JsonProperty("text", Required = Required.Default)]
            public string Text { get; set; }

            [JsonProperty("icon", Required = Required.Default)]
            public string Icon { get; set; }

            [JsonProperty("sound", Required = Required.Default)]
            public string Sound { get; set; }

            [JsonProperty("tag", Required = Required.Default)]
            public string Tag { get; set; }

            [JsonProperty("color", Required = Required.Default)]
            public string Color { get; set; }

            [JsonProperty("click_action", Required = Required.Default)]
            public string ClickAction { get; set; }

            [JsonProperty("body_loc_key", Required = Required.Default)]
            public string BodyLocKey { get; set; }

            [JsonProperty("body_loc_args", Required = Required.Default)]
            public JArray BodyLocArgs { get; set; }

            [JsonProperty("title_loc_key", Required = Required.Default)]
            public string TitleLocKey { get; set; }

            [JsonProperty("title_loc_args", Required = Required.Default)]
            public JArray TitleLocArgs { get; set; }
        }

        [JsonProperty("notification", Required = Required.Default)]
        public NotificationPayload Notification { get; set; }
    }
}
