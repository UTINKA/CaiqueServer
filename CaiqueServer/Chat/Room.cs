using CaiqueServer.Firebase;
using CaiqueServer.Firebase.Json;
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
            await Database.Client.SetAsync($"topics/{Topic}/info", Info);
        }
        
        internal void Distribute(Event Event, string Priority = "normal")
        {
            var SubTopic = Interlocked.Increment(ref Roulette) % MaxRoulette;
            Messaging.Send(new SendMessage
            {
                To = $"/topics/chat-{Topic}-{SubTopic}",
                Data = Event,
                Priority = Priority
            });
        }
    }
}
