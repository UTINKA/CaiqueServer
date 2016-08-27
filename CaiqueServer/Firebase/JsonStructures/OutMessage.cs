using Newtonsoft.Json;

namespace CaiqueServer.Firebase.JsonStructures
{
    [JsonObject(MemberSerialization.OptIn)]
    class OutMessage
    {
        [JsonProperty("to", Required = Required.Always)]
        public string To { get; set; }

        [JsonProperty("message_id", Required = Required.Always)]
        public string MessageId { get; set; }

        [JsonObject(MemberSerialization.OptIn)]
        public class NotificationStructure
        {
            [JsonProperty("title", Required = Required.Always)]
            public string Title { get; set; }

            [JsonProperty("text", Required = Required.Always)]
            public string Text { get; set; }
        }

        [JsonProperty("notification", Required = Required.Default)]
        public NotificationStructure Notification { get; set; }

        [JsonProperty("time_to_live", Required = Required.Default)]
        public string TTL { get; set; }

        [JsonProperty("delay_while_idle", Required = Required.Default)]
        public bool DelayIdle { get; set; }

        [JsonProperty("delivery_receipt_requested", Required = Required.Default)]
        public bool ReceiptRequested { get; set; }
    }
}
