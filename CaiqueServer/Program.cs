using System;
using System.Threading.Tasks;

namespace CaiqueServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Firebase.CloudMessaging.Start();
            ConsoleEvents.SetHandler(delegate
            {
                Console.WriteLine("Shutting down..");
                Task.Delay(1000).Wait();
            });

            Task.Delay(int.MaxValue).Wait();
        }
    }
}
