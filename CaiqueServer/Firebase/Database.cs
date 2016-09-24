using FireSharp;
using FireSharp.Config;
using FireSharp.EventStreaming;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Threading.Tasks;

namespace CaiqueServer.Firebase
{
    class Database
    {
        public static IFirebaseClient Client;
        static EventStreamResponse UserEvents;

        static Database()
        {
            IFirebaseConfig Config = new FirebaseConfig
            {
                AuthSecret = "t7UwJrG90NQADJa6tT3f50JsqkN1c4frnDOdm6SX",
                BasePath = "https://fir-caique.firebaseio.com"
            };

            Client = new FirebaseClient(Config);
        }

        public static async Task Load()
        {
            UserEvents = await Client.OnAsync("users", OnAddUsers, OnChangedUsers, OnRemovedUsers).ConfigureAwait(false);
        }

        public static void Stop()
        {
            UserEvents.Dispose();
            UserEvents = null;
        }

        static void OnAddUsers(object s, ValueAddedEventArgs args, object context)
        {
            Console.WriteLine("Added " + args.Path + " " + args.Data);
        }

        static void OnChangedUsers(object s, ValueChangedEventArgs args, object context)
        {
            Console.WriteLine("Changed " + args.Path + " " + args.Data);
        }

        static void OnRemovedUsers(object s, ValueRemovedEventArgs args, object context)
        {
            Console.WriteLine("Removed " + args.Path);
        }
    }
}
