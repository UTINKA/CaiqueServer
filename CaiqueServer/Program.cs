using CaiqueServer.Music;
using System;
using System.Threading.Tasks;

namespace CaiqueServer
{
    class Program
    {
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
            UpdateData["test"] = "from App Update";

            var NotificationData = new Newtonsoft.Json.Linq.JObject();
            NotificationData["test"] = "from Notification";

            Streamer.Get(0).Enqueue(Song);

            while (true)
            {

                Firebase.CloudMessaging.Send(new Firebase.JsonStructures.SendMessage
                {
                    To = "/topics/1",
                    MessageId = MessageId++.ToString(),
                    Data = UpdateData
                });

                Firebase.CloudMessaging.Send(new Firebase.JsonStructures.SendMessage
                {
                    To = "/topics/1",
                    MessageId = MessageId++.ToString(),
                    Notification = new Firebase.JsonStructures.SendMessage.NotificationPayload
                    {
                        Title = "Topic Test",
                        Text = "Topic 1"
                    },
                    Data = NotificationData
                });

                Task.Delay(15000).Wait();
            }
        }
    }
}
