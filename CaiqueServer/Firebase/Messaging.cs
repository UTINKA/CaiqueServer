using agsXMPP;
using agsXMPP.Net;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using CaiqueServer.Firebase.JsonStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CaiqueServer.Firebase
{
    class Messaging
    {
        static XmppClientConnection Xmpp;

        static Messaging()
        {
            Xmpp = new XmppClientConnection
            {
                UseSSL = true,
                UseStartTLS = false,
                Server = "gcm.googleapis.com",
                Port = 5235,
                Username = "420728598029",
                Password = "AIzaSyDyTRmF8eFLno9UKLdD4bJfSUzLi_eWyeA",
                AutoResolveConnectServer = true,
                SocketConnectionType = SocketConnectionType.Direct,
                AutoAgents = false,
                KeepAlive = true,
                AutoRoster = false,
                AutoPresence = false,
                UseCompression = false,
                Show = ShowType.chat
            };
            
            Xmpp.OnMessage += OnMessage;
            Xmpp.OnError += OnError;
        }

        internal static async Task<bool> Start()
        {
            var T = new TaskCompletionSource<bool>();
            Xmpp.OnLogin += (s) => T.TrySetResult(true);
            Xmpp.OnAuthError += (s, e) => T.TrySetResult(false);
            Xmpp.Open();
            return await T.Task;
        }

        internal static async Task Stop()
        {
            var T = new TaskCompletionSource<bool>();
            Xmpp.OnClose += (s) => T.TrySetResult(true);
            Xmpp.Close();
            await T.Task;
        }

        internal static Dictionary<int, SendMessage> WaitAck = new Dictionary<int, SendMessage>();
        internal static int StaticMessageId = 0;
        internal static int Acks = 0;
        internal static int Sent = 0;

        private static void OnMessage(object s, Message msg)
        {
            var JData = JObject.Parse(msg.FirstChild.Value);

            if (JData["message_type"] == null)
            {
                var Message = JData.ToObject<UpstreamMessage>();
                Send(new ReceivedMessageAck
                {
                    To = Message.From,
                    MessageId = Message.MessageId,
                    MessageType = "ack"
                });

                MessagingHandlers.Upstream(Message);
            }
            else if (JData["message_type"].ToString().EndsWith("ack"))
            {
                var SentAck = JData.ToObject<SentMessageAck>();
                int MessageId;
                if (int.TryParse(SentAck.MessageId, out MessageId) && WaitAck.ContainsKey(MessageId))
                {
                    if (SentAck.MessageType == "ack")
                    {
                        Interlocked.Increment(ref Acks);
                    }
                    else
                    {
                        Interlocked.Decrement(ref Sent);
                        Console.WriteLine("Message " + MessageId + " status " + SentAck.MessageType + " - trying again in 1.5s");

                        var Message = WaitAck[MessageId];
                        Task.Delay(1500).ContinueWith(delegate
                        {
                            Send(Message);
                        });
                    }

                    WaitAck.Remove(MessageId);
                }
            }
            else
            {
                var Message = JData.ToObject<ServerMessage>();
                Send(new ReceivedMessageAck
                {
                    To = Message.From,
                    MessageId = Message.MessageId,
                    MessageType = "ack"
                });
                
                MessagingHandlers.Server(Message);
            }
        }

        private static void OnError(object s, Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        internal static void Send(SendMessage ToSend)
        {
            var MessageId = Interlocked.Increment(ref Messaging.StaticMessageId);
            ToSend.MessageId = MessageId.ToString();
            WaitAck.Add(MessageId, ToSend);
            Send((object)ToSend);
            Interlocked.Increment(ref Sent);
        }

        private static void Send(object JsonObject)
        {
            var Gcm = new Element
            {
                TagName = "gcm",
                Value = JsonConvert.SerializeObject(JsonObject)
            };

            Gcm.Attributes["xmlns"] = "google:mobile:data";

            var Msg = new Element("message");
            Msg.AddChild(Gcm);
            Msg.Attributes["id"] = string.Empty;

            Xmpp.Send(Msg);
        }
    }
}
