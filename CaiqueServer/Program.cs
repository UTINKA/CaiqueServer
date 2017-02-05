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
            
            if (!Cloud.Messaging.Start().Result)
            {
                Console.WriteLine("Auth Error!");
            }
            
            ConsoleEvents.SetHandler(delegate
            {
                Console.WriteLine("Shutting down..");

                var StopFCM = Cloud.Messaging.Stop();
                Cloud.Database.Stop();
                Streamer.Shutdown();
                IcecastProcess.Dispose();
                StopFCM.Wait();
            });

            Console.WriteLine("Boot");

            while (true)
            {
                Console.Title = "Caique " + Cloud.Messaging.Acks + " " + Cloud.Messaging.WaitAck.Count + " " + Cloud.Messaging.Saves;
                Task.Delay(100).Wait();
            }
        }
    }
}
