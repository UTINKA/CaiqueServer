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

                Console.WriteLine("Shutdown complete");
                Task.Delay(500).Wait();
            });

            var Song = Songdata.Search("Nano Gallows Bell")[1];
            var Rand = new Random();

            while (true)
            {
                Task.Delay(5000).Wait();
                Streamer.Get(Rand.Next(0, 10000)).Enqueue(Song);
            }
        }
    }
}
