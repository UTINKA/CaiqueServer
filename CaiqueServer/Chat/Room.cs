using CaiqueServer.Firebase;
using CaiqueServer.Firebase.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CaiqueServer.Chat
{
    class Room
    {
        private string Topic;
        private int Roulette = 0;
        private int MaxRoulette = 32;

        internal Room(string Id)
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
                To = $"/topics/%{Topic}%{SubTopic}",
                Data = Event,
                Priority = Priority
            });
        }
    }
}
