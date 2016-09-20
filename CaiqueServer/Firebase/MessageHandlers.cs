using CaiqueServer.Firebase.JsonStructures;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;

namespace CaiqueServer.Firebase
{
    class MessageHandlers
    {
        private static long MsgId = new Random().Next();

        public static void Upstream(UpstreamMessage In)
        {
            Console.WriteLine("-- Upstream Message " + In.Data ?? string.Empty);
            var ResponseId = Interlocked.Increment(ref MsgId).ToString();

            var ResponseData = new JObject();
            ResponseData["id"] = ResponseId;

            CloudMessaging.Send(new SendMessage
            {
                To = In.From,
                MessageId = ResponseId,
                /*Notification = new SendMessage.NotificationPayload
                {
                    Title = "From C#",
                    Text = "Yes we can! " + ResponseId
                },*/
                Data = ResponseData
            });
        }

        public static void SentAck(SentMessageAck Response)
        {
            if (Response.MessageType == "ack")
            {
                Console.WriteLine("-- Message " + Response.MessageId + " acknowledged");
            }
            else
            {
                Console.WriteLine("-- Message " + Response.MessageId + " not acknowledged - " + Response.ToString());
            }
        }

        public static void Server(ServerMessage Info)
        {
            if (Info.MessageType == "receipt")
            {
                Console.WriteLine("-- Receipt " + Info.MessageId);
            }
            else
            {
                Console.WriteLine("-- CCS " + Info.ControlType);
            }
        }
    }
}
