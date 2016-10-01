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

            //AdaptiveKind AudioBitrate FileExtension Format IsEncrypted Resolution

            //None 192 .mp4 Mp4 False 720 -> NO DROP
            //Streamer.Get("-KSqbu0zMurmthzBE7GF").Enqueue("https://www.youtube.com/watch?v=NqxJ191ecPQ");

            //None 192 .mp4 Mp4 False 720 -> NO DROP
            //Streamer.Get("-KSqbu0zMurmthzBE7GF").Enqueue("https://www.youtube.com/watch?v=SuA1FCCprJA");

            //Audio 128 .mp4 Mp4 True -1 -> NO DROP
            //Streamer.Get("-KSqbu0zMurmthzBE7GF").Enqueue("https://www.youtube.com/watch?v=Q2HmP6Sl5RQ");

            //Audio 128 .mp4 Mp4 True -1 -> NO DROP
            //Streamer.Get("-KSqbu0zMurmthzBE7GF").Enqueue("https://www.youtube.com/watch?v=klUfRsd20gE");

            //Audio 128 .mp4 Mp4 False -1 -> DROP
            //None 96 .mp4 Mp4 False 360 -> DROP
            //Streamer.Get("-KSqbu0zMurmthzBE7GF").Enqueue("initial d running in the 90s");

            //None 192 .mp4 Mp4 False 720 -> NO DROP
            //Streamer.Get("-KSqbu0zMurmthzBE7GF").Enqueue("https://www.youtube.com/watch?v=XMXgHfHxKVM");


            Console.WriteLine("Boot");

            while (true)
            {
                Task.Delay(1000).Wait();
            }
        }
    }
}
