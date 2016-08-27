using agsXMPP;
using agsXMPP.Net;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using CaiqueServer.Firebase.JsonStructures;
using Newtonsoft.Json;
using System;
using System.Text;

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
                Port = 5236,
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

            Xmpp.OnReadSocketData += Xmpp_OnReadSocketData;
            Xmpp.OnWriteSocketData += Xmpp_OnWriteSocketData;
            Xmpp.OnLogin += Xmpp_OnLogin;
            Xmpp.OnAuthError += Xmpp_OnAuthError;
            Xmpp.OnMessage += Xmpp_OnMessage;
            Xmpp.OnError += Xmpp_OnError;
            Xmpp.OnClose += Xmpp_OnClose;
        }

        internal static void Start()
        {
            Xmpp.Open();
        }

        private static void Xmpp_OnWriteSocketData(object sender, byte[] data, int count)
        {
            var text = Encoding.ASCII.GetString(data, 0, count);
            Console.WriteLine("out " + text);
        }

        private static void Xmpp_OnReadSocketData(object sender, byte[] data, int count)
        {
            var text = Encoding.ASCII.GetString(data, 0, count);
            Console.WriteLine("in " + text);
        }

        private static void Xmpp_OnLogin(object sender)
        {
            Console.WriteLine("Logged in");

            var Gcm = new Element("gcm");
            Gcm.Attributes["xmlns"] = "google:mobile:data";
            //Gcm.Value = "{ \"to\" : \"1\", \"message_id\" : \"1\", \"notification\" : { \"title\" : \"Test\", \"text\" : \"Test 2\" }}";

            Gcm.Value = JsonConvert.SerializeObject(new OutMessage
            {
                To = "1",
                MessageId = "1",
                Notification = new OutMessage.NotificationStructure
                {
                    Title = "Test",
                    Text = "Test2"
                }
            });

            var Msg = new Element("message");
            Msg.AddChild(Gcm);
            Msg.Attributes["id"] = string.Empty;
            Xmpp.Send(Msg);
        }

        private static void Xmpp_OnAuthError(object sender, Element e)
        {
            Console.WriteLine("Login failed");
        }

        private static void Xmpp_OnMessage(object sender, Message msg)
        {
            if (!msg.Attributes.Contains("type") || (string)msg.Attributes["type"] != "error")
            {
                var Content = JsonConvert.DeserializeObject<InMessage>(msg.FirstChild.Value);
                Console.WriteLine("Message from " + Content.From);
            }
        }

        private static void Xmpp_OnError(object sender, Exception ex)
        {

        }

        private static void Xmpp_OnClose(object sender)
        {

        }
    }
}
