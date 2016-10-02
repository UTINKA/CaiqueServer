using CaiqueServer.Firebase.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaiqueServer.Firebase
{
    class MessagingHandlers
    {
        private static ConcurrentDictionary<string, string> TokenCache = new ConcurrentDictionary<string, string>();

        private static async Task SendChatList(string Id, string Token)
        {
            var Chats = await Database.Client.GetAsync($"member/{Id}");
            var List = Chats.ResultAs<Dictionary<string, bool>>();

            if (List != null)
            {
                Messaging.Send(new SendMessage
                {
                    To = Token,
                    Data = new { type = "list", chats = List.Keys }
                });
            }
        }

        public static async Task Upstream(UpstreamMessage In)
        {
            var Event = In.Data.ToObject<Event>();
            if (!TokenCache.TryGetValue(In.From, out Event.Sender))
            {
                Event.Sender = Database.Client.Get($"token/{In.From}/key").ResultAs<string>();
                if (Event.Sender != null)
                {
                    TokenCache.TryAdd(In.From, Event.Sender);
                }
            }

            if (Event.Sender == null)
            {
                Console.WriteLine("Not registered - " + In.Data.ToString());
                if (Event.Type == "reg" && Event.Text != null)
                {
                    var Userdata = await Authentication.UserdataFromToken(Event.Text);
                    Console.WriteLine("Register with " + Userdata.Email);

                    await Database.Client.SetAsync($"token/{In.From}", new { key = Userdata.Sub });
                    if ((await Database.Client.GetAsync($"user/{Userdata.Sub}")).ResultAs<DatabaseUser>() == null)
                    {
                        await Database.Client.SetAsync($"user/{Userdata.Sub}", new DatabaseUser
                        {
                            Name = Userdata.Name,
                            Picture = Userdata.Picture
                        });

                        await Database.Client.SetAsync($"member/{Userdata.Sub}/-KSqbu0zMurmthzBE7GF", true);

                        Messaging.Send(new SendMessage
                        {
                            To = In.From,
                            Data = new Event
                            {
                                Type = "regdone",
                                Text = "Registered as " + Userdata.Name + " and auto-joined the starting chat"
                            }
                        });
                    }

                    await SendChatList(Userdata.Sub, In.From);
                }
            }
            else
            {
                switch (Event.Type)
                {
                    case "text":
                        Chat.Home.ById(Event.Chat).Distribute(Event, "high");
                        break;

                    case "madd":
                        Music.Streamer.Get(Event.Chat).Enqueue(Event.Text, Event.Sender);
                        break;

                    case "msearch":
                        Messaging.Send(new SendMessage
                        {
                            To = In.From,
                            Data = new { type = "mres", r = Music.Songdata.Search(Event.Text) }
                        });
                        break;

                    case "mskip":
                        Music.Streamer.Get(Event.Chat).Skip();
                        break;

                    case "reg":
                        await SendChatList(Event.Sender, In.From);
                        break;

                    case "profile":
                        /*Messaging.Send(new SendMessage
                        {
                            To = In.From,
                            Data = User
                        });*/
                        break;

                    case "newchat":
                        var Id = await Database.Client.PushAsync("chat", new DatabaseChat
                        {
                            Title = Event.Text,
                            Picture = "893b5376-6e5a-4699-bb71-f360c6ebe8d7",
                            Tags = new[] { "test" }
                        });

                        await Database.Client.SetAsync($"member/{Event.Sender}/{Id.Result.Name}", true);
                        break;

                    case "update":
                        //ToDo: Update DB
                        //Chat.Home.ById(Event.Chat).Distribute(Event);
                        break;
                }

                var User = Database.Client.Get($"user/{Event.Sender}").ResultAs<DatabaseUser>();
                Console.WriteLine("-- Upstream from " + User.Name + " " + Event.Type + " " + Event.Text);
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
