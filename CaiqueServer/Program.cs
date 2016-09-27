using CaiqueServer.Music;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CaiqueServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Caique Server";

            var ProcessStartInfo = new ProcessStartInfo
            {
                FileName = "Includes/icecast.exe",
                Arguments = "-c Includes/icecast.xml",
                UseShellExecute = false,
                RedirectStandardOutput = false
            };

            var IcecastProcess = Process.Start(ProcessStartInfo);
            IcecastProcess.PriorityClass = ProcessPriorityClass.AboveNormal;

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
                IcecastProcess.Dispose();
                StopFCM.Wait();
            });

            Task.Run(async delegate
            {
                while (true)
                {
                    Console.Title = "Caique " + Firebase.Messaging.Acks + " " + Firebase.Messaging.WaitAck.Count + " " + Firebase.Messaging.Saves;
                    await Task.Delay(100);
                }
            });

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
            
            Firebase.Database.Client.Set("chat/0", new Firebase.Json.DatabaseChat
            {
                Title = "Test Chat",
                Tags = new[] { "test", "programming" },
                Picture = 5
            });

            Console.WriteLine("Start spam");

            for (int i = 0; i < 50; i++)
            {
                var ChatId = i % 10;

                Chat.Home.ById(ChatId).Distribute(new Firebase.Json.Event
                {
                    Chat = ChatId,
                    Sender = 2,
                    Type = "text",
                    Text = "Hi!",
                    Date = 1474402074,
                    Attachment = i
                }, "high");
            }
            
            Console.WriteLine("Boot");

            for (int i = 0; i < 10; i++)
            {
                Streamer.Get(i).Enqueue("trash candy");
            }

            while (true)
            {
                Task.Delay(1000).Wait();
            }
        }
    }
}
