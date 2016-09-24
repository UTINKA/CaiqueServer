using CaiqueServer.Firebase.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace CaiqueServer.Firebase
{
    class MessagingHandlers
    {
        public static async Task Upstream(UpstreamMessage In)
        {
            var Sender = Database.Client.Get($"token/{In.From}").ResultAs<DatabaseToken>();
            if (Sender == null)
            {
                Console.WriteLine("-- Token " + In.From + " not in DB - " + In.Data ?? string.Empty);
            }
            else
            {
                try
                {
                    var Message = In.Data.ToObject<DatabaseMessage>();
                    Message.Sender = Sender.Id;

                    var User = Database.Client.Get($"user/{Sender.Id}").ResultAs<DatabaseUser>();
                    Console.WriteLine("-- Upstream from " + User.Name + " " + JsonConvert.SerializeObject(Message));

                    if (Message.Type == "text")
                    {
                        await Chat.Home.ById(Message.Chat).Distribute(Message);
                    }
                    else if (Message.Type == "update")
                    {
                        var Update = JsonConvert.DeserializeObject<DatabaseChat>(Message.Text);
                        await Chat.Home.ById(Message.Chat).Distribute(Message);
                    }
                    else
                    {
                        //Personal?

                        var ResponseData = new JObject();
                        ResponseData["list"] = "yes";

                        Messaging.Send(new SendMessage
                        {
                            To = In.From,
                            Data = ResponseData
                        });
                    }
                }
                catch (Exception Ex)
                {
                    Console.WriteLine("Invalid Message " + Ex.ToString());

                    var ResponseData = new JObject();
                    ResponseData["received"] = true;

                    Messaging.Send(new SendMessage
                    {
                        To = In.From,
                        Data = ResponseData
                    });
                }
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
