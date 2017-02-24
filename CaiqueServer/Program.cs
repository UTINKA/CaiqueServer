using CaiqueServer.Music;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MusicSearch;
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

            SongRequest.YouTube = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = "AIzaSyAVrXiAHfLEbQbNJP80zbTuW2jL0wuEigQ"
            });

            SongRequest.SoundCloud = "5c28ed4e5aef8098723bcd665d09041d";

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
