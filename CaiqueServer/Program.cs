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
            Ffmpeg.PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;

            Console.WriteLine("Icecast server started");
            
            if (!Firebase.Messaging.Start().Result)
            {
                Console.WriteLine("Auth Error!");
            }
            
            ConsoleEvents.SetHandler(delegate
            {
                Console.WriteLine("Shutting down..");

                var StopFCM = Firebase.Messaging.Stop();
                Firebase.Database.Stop();
                Streamer.Shutdown();
                StopFCM.Wait();
                Ffmpeg.Dispose();

                Console.WriteLine("Shutdown complete");
                Task.Delay(100).Wait();
            });

            Task.Run(async delegate
            {
                while (true)
                {
                    Console.Title = "Caique " + Firebase.Messaging.Acks + " " + Firebase.Messaging.WaitAck.Count + " " + Firebase.Messaging.Saves;
                    await Task.Delay(100);
                }
            });

            /*var Song = Songdata.Search("Nano Gallows Bell")[1];
            for (int i = 0; i < 10; i++)
            {
                Streamer.Get(i).Enqueue(Song);
            }*/


            //ToDo: Automate this IN THE APP
            var KlteToken = "c-AxRCHxqH4:APA91bEl7bgursLXdnuOGRnvPk83_X0E-WrKkA-4cxGYsaysVKXAj-s69mY6UjmuF2D6y3FqPCjX3I0k8FCLRjfrI-n8HhjJuQG3O58WGkf9HiqJJzvIocuvkLUaiXJnmtA7e2DbRL0u";

            Firebase.Database.Client.Set("user/2", new Firebase.Json.DatabaseUser
            {
                Mail = "amirzaidi1999@gmail.com",
                Token = KlteToken,
                Name = "Amir Zaidi"
            });

            Firebase.Database.Client.Set("token/" + KlteToken, new Firebase.Json.DatabaseToken
            {
                Id = 2
            });


            Console.WriteLine("Start spam");

            for (int i = 0; i < 200; i++)
            {
                var Msg = new Firebase.Json.DatabaseMessage
                {
                    Chat = i % 10,
                    Sender = 2,
                    Type = "text",
                    Text = "Hi!",
                    Date = 1474402074,
                    Attachment = i
                };

                Chat.Home.ById(i % 10).Distribute(Msg);
            }
            
            Console.WriteLine("Boot");

            while (true)
            {
                Task.Delay(1000).Wait();
            }
        }
    }
}
