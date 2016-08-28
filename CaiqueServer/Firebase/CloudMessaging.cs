using agsXMPP;
using agsXMPP.Net;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using CaiqueServer.Firebase.JsonStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading;

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
            Xmpp.OnLogin += OnLogin;
            Xmpp.OnAuthError += OnAuthError;
            Xmpp.OnMessage += OnMessage;
            Xmpp.OnError += OnError;
            Xmpp.OnClose += OnClose;
        }

        internal static void Start()
        {
            Xmpp.Open();
        }

        private static void OnWriteSocketData(object sender, byte[] data, int count)
        {
            var text = Encoding.ASCII.GetString(data, 0, count);
            Console.WriteLine("-- Out " + text);
        }

        private static void OnReadSocketData(object sender, byte[] data, int count)
        {
            var text = Encoding.ASCII.GetString(data, 0, count);
            Console.WriteLine("-- In " + text);
        }

        private static void OnLogin(object sender)
        {
            Console.WriteLine("Logged in");
            //"{ \"to\" : \"1\", \"message_id\" : \"1\", \"notification\" : { \"title\" : \"Test\", \"text\" : \"Test 2\" }}";
        }

        private static void OnAuthError(object sender, Element e)
        {
            Console.WriteLine("Login failed");
        }

        private static long MsgId = 1;

        private static void OnMessage(object sender, Message msg)
        {
            var JData = JObject.Parse(msg.FirstChild.Value);
            if (JData["message_type"] == null)
            {
                var Message = JData.ToObject<InMessage>();
                Console.WriteLine("-- Message " + Message.Data ?? string.Empty);

                var ResponseId = Interlocked.Increment(ref MsgId).ToString();
                var ResponseData = new JObject();
                ResponseData["id"] = ResponseId;

                Xmpp.Send(FromJson(JsonConvert.SerializeObject(Message.GetResponse())));
                Xmpp.Send(FromJson(JsonConvert.SerializeObject(new OutMessage
                {
                    To = Message.From,
                    MessageId = ResponseId,
                    Notification = new OutMessage.NotificationPayload
                    {
                        Title = "From C#",
                        Text = "Yes we can! " + ResponseId
                    },
                    Data = ResponseData
                })));
            }
            else
            {
                var Message = JData.ToObject<CCSMessage>();
                Console.WriteLine("-- CCS Message " + Message.MessageType);
            }
        }

        private static void OnError(object sender, Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        private static void OnClose(object sender)
        {
            Console.WriteLine("Closed");
        }

        private static Element FromJson(string Json)
        {
            var Gcm = new Element
            {
                TagName = "gcm",
                Value = Json
            };
            Gcm.Attributes["xmlns"] = "google:mobile:data";

            var Msg = new Element("message");
            Msg.AddChild(Gcm);
            Msg.Attributes["id"] = string.Empty;

            return Msg;
        }
    }
}
