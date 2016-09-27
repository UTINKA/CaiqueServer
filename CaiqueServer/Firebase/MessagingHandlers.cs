using CaiqueServer.Firebase.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CaiqueServer.Firebase
{
    class MessagingHandlers
    {
        public static void Upstream(UpstreamMessage In)
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
                    var Event = In.Data.ToObject<Event>();
                    Event.Sender = Sender.Id;

                    var User = Database.Client.Get($"user/{Sender.Id}").ResultAs<DatabaseUser>();
                    Console.WriteLine("-- Upstream from " + User.Name + " " + JsonConvert.SerializeObject(In.Data));

                    if (Event.Type == "text")
                    {
                        Chat.Home.ById(Event.Chat).Distribute(Event, "high");
                    }
                    else if (Event.Type == "reg")
                    {
                        var ResponseData = new JObject();
                        ResponseData["received"] = "Registration";

                        Messaging.Send(new SendMessage
                        {
                            To = In.From,
                            Data = ResponseData
                        });
                    }
                    else if (Event.Type == "profile")
                    {
                        var ResponseData = new JObject();
                        ResponseData["received"] = "Profile Update";

                        Messaging.Send(new SendMessage
                        {
                            To = In.From,
                            Data = ResponseData
                        });
                    }
                    else if (Event.Type == "msearch")
                    {
                        Messaging.Send(new SendMessage
                        {
                            To = In.From,
                            Data = new { r = Music.Songdata.Search(Event.Text) }
                        });
                    }
                    else
                    {
                        if (Event.Type == "update")
                        {
                            var Update = JsonConvert.DeserializeObject<DatabaseChat>(Event.Text);
                            //ToDo: Update DB
                        }
                        else if (Event.Type == "madd")
                        {
                            Music.Streamer.Get(Event.Chat).Enqueue(Event.Text);
                        }
                        else if (Event.Type == "mskip")
                        {
                            Music.Streamer.Get(Event.Chat).Skip();
                        }

                        Chat.Home.ById(Event.Chat).Distribute(Event);
                        //Personal?
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
