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

            Streamer.Get("-KSqbu0zMurmthzBE7GF").Enqueue("gallows bell");

            Console.WriteLine("Boot");

            while (true)
            {
                Task.Delay(1000).Wait();
            }
        }
    }
}
