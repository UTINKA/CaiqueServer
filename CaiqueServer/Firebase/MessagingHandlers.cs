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
            var Event = In.Data.ToObject<Event>();
            var Key = Database.Client.Get($"token/{In.From}/key").ResultAs<string>();
            if (Key == null)
            {
                Console.WriteLine("Not registered - " + In.Data.ToString());
                if (Event.Type == "reg" && Event.Text != null)
                {
                    var Userdata = await Authentication.GetUnique(Event.Text);
                    Key = Userdata.Sub;

                    Console.WriteLine("Register with " + Userdata.Email);

                    await Database.Client.SetAsync($"token/{In.From}", new { key = Key });
                    if ((await Database.Client.GetAsync($"user/{Key}")).ResultAs<DatabaseUser>() == null)
                    {
                        await Database.Client.SetAsync($"user/{Key}", new DatabaseUser
                        {
                            Name = Userdata.Name
                        });
                    }
                }
            }
            else
            {
                Event.Sender = Key;
                var User = Database.Client.Get($"user/{Key}").ResultAs<DatabaseUser>();
                if (Event.Type == "reg")
                {
                    Console.WriteLine("-- " + User.Name + " started the app");
                    Messaging.Send(new SendMessage
                    {
                        To = In.From,
                        Data = new Event
                        {
                            Type = "welcome",
                            Text = "Welcome " + User.Name
                        }
                    });
                }
                else
                {
                    Console.WriteLine("-- Upstream from " + User.Name + " " + JsonConvert.SerializeObject(In.Data));

                    if (Event.Type == "text")
                    {
                        Chat.Home.ById(Event.Chat).Distribute(Event, "high");
                    }
                    else if (Event.Type == "madd")
                    {
                        Music.Streamer.Get(Event.Chat).Enqueue(Event.Text);
                    }
                    else if (Event.Type == "msearch")
                    {
                        Messaging.Send(new SendMessage
                        {
                            To = In.From,
                            Data = new { r = Music.Songdata.Search(Event.Text) }
                        });
                    }
                    else if (Event.Type == "mskip")
                    {
                        Music.Streamer.Get(Event.Chat).Skip();
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
                    else
                    {
                        if (Event.Type == "update")
                        {
                            var Update = JsonConvert.DeserializeObject<DatabaseChat>(Event.Text);
                            //ToDo: Update DB
                        }

                        Chat.Home.ById(Event.Chat).Distribute(Event);
                    }
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
