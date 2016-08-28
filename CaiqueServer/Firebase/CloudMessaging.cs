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
        }

        private static void OnAuthError(object sender, Element e)
        {
            Console.WriteLine("Login failed");
        }

        private static void OnMessage(object sender, Message msg)
        {
            var JData = JObject.Parse(msg.FirstChild.Value);
            if (JData["message_type"] == null)
            {
                var In = JData.ToObject<InMessage>();
                Send(In.GetAck());
                MessageHandlers.OnInMessage(In);
            }
            else
            {
                MessageHandlers.OnOutMessageResponse(JData.ToObject<OutMessageResponse>());
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
