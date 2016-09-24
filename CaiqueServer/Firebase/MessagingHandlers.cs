using CaiqueServer.Firebase.JsonStructures;
using Newtonsoft.Json.Linq;
using System;

namespace CaiqueServer.Firebase
{
    class MessagingHandlers
    {
        public static void Upstream(UpstreamMessage In)
        {
            Console.WriteLine("-- Upstream Message " + In.Data ?? string.Empty);

            var ResponseData = new JObject();
            ResponseData["received"] = "true";

            Messaging.Send(new SendMessage
            {
                To = In.From,
                Data = ResponseData
            });
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
