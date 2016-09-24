using CaiqueServer.Music;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CaiqueServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Caique Server";
            var DbConnecter = Firebase.Database.Load();

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

            DbConnecter.Wait();
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

            /*var Song = Songdata.Search("Nano Gallows Bell")[1];
            var Rand = new Random();
            */
            var UpdateData = new Newtonsoft.Json.Linq.JObject();
            UpdateData["content"] = "Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!Test message sent from the server!";
            UpdateData["user"] = "Amir Zaidi";
            UpdateData["utctime"] = "1474402074";
            /*
            for (int i = 0; i < 10; i++)
            {
                Streamer.Get(i).Enqueue(Song);
            }*/

            Console.WriteLine("Booted!");

            /*while (true)
            {
                Firebase.Database.Client.Delete("users");
                Firebase.Database.Client.Set("users/1", new Firebase.JsonStructures.User
                {
                    Mail = "amirzaidiamirzaidi@gmail.com",
                    Token = "eGFRIQbqVk0:APA91bEr5lb5TMB9lNEacZSOGEpOkFZsRuhcF-GzakhussyYBhcZZ5NiGc0qKaZ6qDpsjeh_vyhhFqE6LNJd4dpaO-vd36fR3HJ5ThcYoyX7MgfFvF44tD94qkjz4yQx7lrIqcpPybuv",
                    Name = "Amir AVD"
                });

                Firebase.Database.Client.Set("users/2", new Firebase.JsonStructures.User
                {
                    Mail = "amirzaidi1999@gmail.com",
                    Token = "c-AxRCHxqH4:APA91bEl7bgursLXdnuOGRnvPk83_X0E-WrKkA-4cxGYsaysVKXAj-s69mY6UjmuF2D6y3FqPCjX3I0k8FCLRjfrI-n8HhjJuQG3O58WGkf9HiqJJzvIocuvkLUaiXJnmtA7e2DbRL0u",
                    Name = "Amir S5"
                });
                Console.WriteLine(Firebase.Database.Client.Get("users/1").ResultAs<Firebase.JsonStructures.User>().Name);
                Console.WriteLine(Firebase.Database.Client.Get("users/2").ResultAs<Firebase.JsonStructures.User>().Name);
                Console.Title = "Caique " + Firebase.Messaging.Acks + " " + Firebase.Messaging.WaitAck.Count + " " + Firebase.Messaging.Sent;
            }*/

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    Firebase.Messaging.Send(new Firebase.JsonStructures.SendMessage
                    {
                        To = $"/topics/chat-{i}-{j}",
                        Data = UpdateData
                    });
                }

                Task.Delay(1000).Wait();
                Console.Title = "Caique " + Firebase.Messaging.Acks + " " + Firebase.Messaging.WaitAck.Count + " " + Firebase.Messaging.Sent;
            }

            while (true)
            {
                Task.Delay(1000).Wait();
                Console.Title = "Caique " + Firebase.Messaging.Acks + " " + Firebase.Messaging.WaitAck.Count + " " + Firebase.Messaging.Sent;
            }
        }
    }
}
