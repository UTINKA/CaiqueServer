using agsXMPP;
using agsXMPP.Net;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using CaiqueServer.Firebase.JsonStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CaiqueServer.Firebase
{
    class CloudMessaging
    {
        static XmppClientConnection Xmpp;

        static CloudMessaging()
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

            Xmpp.OnReadSocketData += OnReadSocketData;
            Xmpp.OnWriteSocketData += OnWriteSocketData;
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

        private static void OnWriteSocketData(object s, byte[] data, int count)
        {
            //var text = Encoding.ASCII.GetString(data, 0, count);
            //Console.WriteLine("-- Out " + text);
        }

        private static void OnReadSocketData(object s, byte[] data, int count)
        {
            //var text = Encoding.ASCII.GetString(data, 0, count);
            //Console.WriteLine("-- In " + text);
        }

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

                MessageHandlers.Upstream(Message);
            }
            else if (JData["message_type"].ToString().EndsWith("ack"))
            {
                MessageHandlers.SentAck(JData.ToObject<SentMessageAck>());
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

                MessageHandlers.Server(Message);
            }
        }

        private static void OnError(object s, Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        internal static void Send(object JsonObject)
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
