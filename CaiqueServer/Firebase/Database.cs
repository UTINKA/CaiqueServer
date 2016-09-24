using FireSharp;
using FireSharp.Config;
using FireSharp.Exceptions;
using System;

namespace CaiqueServer.Firebase
{
    class Database
    {
        public static FirebaseClient Client;

        static Database()
        {
            Client = new FirebaseClient(new FirebaseConfig
            {
                AuthSecret = "t7UwJrG90NQADJa6tT3f50JsqkN1c4frnDOdm6SX",
                BasePath = "https://fir-caique.firebaseio.com",
                RequestTimeout = new TimeSpan(0, 5, 0)
            });

            Client.OnException += OnException;
        }

        private static void OnException(FirebaseException ex)
        {
            Console.WriteLine(ex.ToString());
        }

        public static void Stop()
        {
            Client.Dispose();
        }
    }
}
