using FireSharp;
using FireSharp.Config;
using FireSharp.EventStreaming;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CaiqueServer.Firebase
{
    class Database
    {
        public static IFirebaseClient Client;
        private static long _MessageId;
        private static Timer AutoIdUpdater;
        internal static long MessageId
        {
            get
            {
                return Interlocked.Increment(ref _MessageId);
            }
        }
        //static EventStreamResponse MessageEvents;
        //static EventStreamResponse TokenEvents;
        //static EventStreamResponse Events;

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
            //Events = await Client.OnAsync("", OnAdd, OnChange, OnRemove).ConfigureAwait(false);

            _MessageId = (await Client.GetAsync("lock/msgid")).ResultAs<long>() + 100;
            AutoIdUpdater = new Timer(async delegate
            {
                try
                {
                    await Client.SetAsync("lock/msgid", _MessageId);
                }
                catch //(Exception Ex)
                {
                    //Console.WriteLine(Ex.ToString());
                }
            }, null, 0, 5000);
        }

        public static void Stop()
        {
            //Events?.Dispose();
            //Events = null;
        }

        static void OnAdd(object s, ValueAddedEventArgs args, object context)
        {
            Console.WriteLine("Added " + args.Path + " " + args.Data);
        }

        static void OnChange(object s, ValueChangedEventArgs args, object context)
        {
            Console.WriteLine("Changed " + args.Path + " " + args.Data);
        }

        static void OnRemove(object s, ValueRemovedEventArgs args, object context)
        {
            Console.WriteLine("Removed " + args.Path);
        }
    }
}
