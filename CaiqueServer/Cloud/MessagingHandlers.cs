using CaiqueServer.Cloud.Json;
using MusicSearch;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CaiqueServer.Cloud
{
    class MessagingHandlers
    {
        private static ConcurrentDictionary<string, string> TokenCache = new ConcurrentDictionary<string, string>();

        private static async Task SendChatList(string Id, string Token)
        {
            var Chats = (await Database.Client.GetAsync($"user/{Id}/member")).ResultAs<Dictionary<string, bool>>();

            if (Chats != null)
            {
                Messaging.Send(new SendMessage
                {
                    To = Token,
                    Data = new { type = "list", chats = Chats.Keys }
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
                    //await SendChatList(Userdata.Sub, In.From);

                    if ((await Database.Client.GetAsync($"user/{Userdata.Sub}/data")).ResultAs<DatabaseUser>() == null)
                    {
                        await Database.Client.SetAsync($"user/{Userdata.Sub}/data", new DatabaseUser
                        {
                            Name = Userdata.Name
                        });

                        await new Firebase.Storage.FirebaseStorage("firebase-caique.appspot.com").Child("users").Child(Userdata.Sub).PutAsync(File.Open("Includes/emptyUser.png", FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

                        await Database.Client.SetAsync($"user/{Userdata.Sub}/member/-KSqbu0zMurmthzBE7GF", true);
                    }
                }
                else
                {
                    Messaging.Send(new SendMessage
                    {
                        To = In.From,
                        Data = new Event
                        {
                            Type = "reg"
                        }
                    });
                }
            }
            else
            {
                switch (Event.Type)
                {
                    case "text":
                        Chat.Home.ById(Event.Chat).Distribute(Event, "high");
                        break;

                    case "typing":
                        Chat.Home.ById(Event.Chat).Distribute(Event);
                        break;

                    case "madd":
                        if (await Music.Streamer.Get(Event.Chat).Enqueue(Event.Text, Event.Sender))
                        {
                            Messaging.Send(new SendMessage
                            {
                                To = In.From,
                                Data = new Event
                                {
                                    Chat = Event.Chat,
                                    Type = "play",
                                    Text = Music.Streamer.Serialize(Event.Chat)
                                }
                            });
                        }
                        break;

                    case "msearch":
                        Messaging.Send(new SendMessage
                        {
                            To = In.From,
                            Data = new
                            {
                                chat = Event.Chat,
                                type = "mres",
                                r = await SongRequest.Search(Event.Text)
                            }
                        });
                        break;

                    case "mskip":
                        Music.Streamer.Get(Event.Chat).Skip();
                        break;

                    case "mremove":
                        ushort ToRemove;
                        if (ushort.TryParse(Event.Text, out ToRemove) && ToRemove-- != 0)
                        {
                            if (Music.Streamer.Get(Event.Chat).Queue.TryRemove(ToRemove, out Song Song))
                            {
                                Event.Text = Song.Title;
                                Event.Sender = null;
                                Messaging.Send(new SendMessage
                                {
                                    To = In.From,
                                    Data = Event
                                });
                            }
                        }
                        break;

                    case "mplaying":
                        Messaging.Send(new SendMessage
                        {
                            To = In.From,
                            Data = new Event
                            {
                                Chat = Event.Chat,
                                Type = "play",
                                Text = Music.Streamer.Serialize(Event.Chat)
                            }
                        });
                        break;

                    case "newchat":
                        var ChatData = JsonConvert.DeserializeObject<DatabaseChat>(Event.Text);

                        var Id = await Database.Client.PushAsync("chat", new
                        {
                            data = ChatData
                        });

                        var ChatId = Id.Result.Name.ToString();

                        await new Firebase.Storage.FirebaseStorage("firebase-caique.appspot.com").Child("chats").Child(ChatId).PutAsync(File.Open("Includes/emptyChat.png", FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        await Database.Client.SetAsync($"user/{Event.Sender}/member/{ChatId}", true);

                        foreach (var Tag in ChatData.Tags)
                        {
                            await Database.Client.SetAsync($"tags/{Tag}/{ChatId}", true);
                        }

                        break;

                    case "joinchat":
                        if ((await Database.Client.GetAsync($"chat/{Event.Chat}/data/title")).ResultAs<string>() != null)
                        {
                            await Database.Client.SetAsync($"user/{Event.Sender}/member/{Event.Chat}", true);
                        }

                        break;

                    case "leavechat":
                        await Database.Client.DeleteAsync($"user/{Event.Sender}/member/{Event.Chat}");
                        break;

                    case "searchtag":
                        var Split = Event.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (Split.Length != 0)
                        {
                            Messaging.Send(new SendMessage
                            {
                                To = In.From,
                                Data = new { type = "tagres", r = await Chat.Home.ByTags(Split) }
                            });
                        }
                        break;

                    case "update":
                        (await Chat.Home.ById(Event.Chat).Update(JsonConvert.DeserializeObject<DatabaseChat>(Event.Text))).Distribute(Event);
                        break;

                    case "name":
                        await Database.Client.SetAsync($"user/{Event.Sender}/data", new DatabaseUser
                        {
                            Name = Event.Text
                        });
                        break;
                }

                var User = Database.Client.Get($"user/{Event.Sender}/data").ResultAs<DatabaseUser>();
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} -- Upstream from {User.Name} {Event.Type} {Event.Text}");
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
