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

            Firebase.CloudMessaging.Start();

            ConsoleEvents.SetHandler(delegate
            {
                Console.WriteLine("Shutting down..");

                Streamer.Shutdown.Cancel();

                Task.Delay(int.MaxValue).Wait();
            });

            var Song = Songdata.Search("Nano Gallows Bell")[1];
            var Rand = new Random();

            while (true)
            {
                Task.Delay(5000).Wait();
                Streamer.Get(Rand.Next(0, 10000)).Queue.Enqueue(Song);
            }
        }
    }
}
