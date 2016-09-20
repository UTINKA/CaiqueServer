using CaiqueServer.Music;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CaiqueServer
{
    class Program
    {
        struct JOTo
        {
            public Newtonsoft.Json.Linq.JObject JObject;
            public string Topic;
        }

        static void Main(string[] args)
        {
            Console.Title = "Caique Server";

            var ProcessStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "Includes/icecast.exe",
                Arguments = "-c Includes/icecast.xml",
                UseShellExecute = false,
                RedirectStandardOutput = false
            };

            var Ffmpeg = System.Diagnostics.Process.Start(ProcessStartInfo);
            Ffmpeg.PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;

            Console.WriteLine("Icecast server started");

            if (!Firebase.CloudMessaging.Start().Result)
            {
                Console.WriteLine("Auth Error!");
            }

            ConsoleEvents.SetHandler(delegate
            {
                Console.WriteLine("Shutting down..");

                var StopFCM = Firebase.CloudMessaging.Stop();
                Streamer.Shutdown();
                StopFCM.Wait();
                Ffmpeg.Dispose();

                Console.WriteLine("Shutdown complete");
                Task.Delay(100).Wait();
            });

            var Song = Songdata.Search("Nano Gallows Bell")[1];
            var Rand = new Random();

            var MessageId = 1;

            var UpdateData = new Newtonsoft.Json.Linq.JObject();
            UpdateData["content"] = "Test message sent from the server!";
            UpdateData["user"] = "Amir Zaidi";
            UpdateData["utctime"] = "1474402074";

            //var NotificationData = new Newtonsoft.Json.Linq.JObject();
            //NotificationData["test"] = "Notification";

            for (int i = 0; i < 10; i++)
            {
                Streamer.Get(i).Enqueue(Song);
            }

            Console.WriteLine("Booted!");
            
            var MsgQueue = new System.Collections.Generic.Queue<JOTo>();

            while (true)
            {
                /*Firebase.CloudMessaging.Send(new Firebase.JsonStructures.SendMessage
                {
                    To = "/topics/1",
                    MessageId = MessageId++.ToString(),
                    Data = UpdateData,
                    Priority = "normal"
                });*/

                MsgQueue.Enqueue(new JOTo
                {
                    Topic = "1",
                    JObject = UpdateData
                });

                MsgQueue.Enqueue(new JOTo
                {
                    Topic = "1",
                    JObject = UpdateData
                });

                MsgQueue.Enqueue(new JOTo
                {
                    Topic = "2",
                    JObject = UpdateData
                });

                Task.Delay(750).Wait();
                foreach (var Topic in MsgQueue.GroupBy(x => x.Topic))
                {
                    var Msg = new Firebase.JsonStructures.SendMessage
                    {
                        To = $"/topics/{Topic.Key}",
                        MessageId = MessageId++.ToString(),
                        Data = new Newtonsoft.Json.Linq.JObject(),
                        Priority = "normal"
                    };

                    Msg.Data["updates"] = new Newtonsoft.Json.Linq.JArray(Topic.Select(x => x.JObject));

                    Firebase.CloudMessaging.Send(Msg);
                }
                
                /*
                Firebase.CloudMessaging.Send(new Firebase.JsonStructures.SendMessage
                {
                    To = "/topics/1",
                    MessageId = MessageId++.ToString(),
                    Notification = new Firebase.JsonStructures.SendMessage.NotificationPayload
                    {
                        Title = "Topic Test",
                        Text = "Topic " + MessageId + "\r\nTest\r\nTest\r\nDoes this work?",
                        Tag = "1"
                    },
                    Data = NotificationData,
                    Priority = "normal"
                });
                */
                // Task.Delay(15000).Wait();
            }
        }
    }
}
