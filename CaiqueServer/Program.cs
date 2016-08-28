using CaiqueServer.Music;
using System;
using System.Threading.Tasks;

namespace CaiqueServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var Streamers = new IcecastStreamer[40];
            var Song = Songdata.Search("Nano Gallows Bell")[0];

            for (int i = 0; i < Streamers.Length; i++)
            {
                Streamers[i] = new IcecastStreamer(i);
                Streamers[i].Queue.Enqueue(Song);
                Streamers[i].StreamLoop();
            }

            Firebase.CloudMessaging.Start();

            ConsoleEvents.SetHandler(delegate
            {
                Console.WriteLine("Shutting down..");
                for (int i = 0; i < Streamers.Length; i++)
                {
                    Streamers[i].CloseRequester.Cancel();
                }

                Task.Delay(500).Wait();
            });

            Task.Delay(int.MaxValue).Wait();
        }
    }
}
