using CaiqueServer.Firebase.JsonStructures;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;

namespace CaiqueServer.Firebase
{
    class MessageHandlers
    {
        private static long MsgId = new Random().Next();

        public static void OnInMessage(InMessage In)
        {
            Console.WriteLine("-- Message " + In.Data ?? string.Empty);
            var ResponseId = Interlocked.Increment(ref MsgId).ToString();

            var ResponseData = new JObject();
            ResponseData["id"] = ResponseId;

            CloudMessaging.Send(new OutMessage
            {
                To = In.From,
                MessageId = ResponseId,
                Notification = new OutMessage.NotificationPayload
                {
                    Title = "From C#",
                    Text = "Yes we can! " + ResponseId
                },
                Data = ResponseData
            });
        }

        public static void OnOutMessageResponse(OutMessageResponse Response)
        {
            Console.WriteLine();
        }
    }
}
