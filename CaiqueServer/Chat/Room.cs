using CaiqueServer.Firebase;
using CaiqueServer.Firebase.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CaiqueServer.Chat
{
    class Room
    {
        private int Topic;
        private int Roulette = 0;
        private int MaxRoulette = 32;

        internal Room(int Id)
        {
            Topic = Id;
        }

        internal async Task Update(DatabaseChat Info)
        {
            await Database.Client.SetAsync($"chat/{Topic}/info", Info);
        }
        
        internal async Task Distribute(DatabaseMessage Message)
        {
            try
            {
                var SubTopic = Interlocked.Increment(ref Roulette) % MaxRoulette;
                var Id = Messaging.Send(new SendMessage
                {
                    To = $"/topics/chat-{Topic}-{SubTopic}",
                    Data = Message
                });

                await Database.Client.SetAsync($"chat/{Topic}/{Id}", true).ConfigureAwait(false);
                await Database.Client.SetAsync($"message/{Id}", Message).ConfigureAwait(false);
            }
            catch //(Exception Ex)
            {
                //Console.WriteLine(Ex.ToString());
            }
        }
    }
}
