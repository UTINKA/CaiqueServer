using CaiqueServer.Cloud;
using CaiqueServer.Cloud.Json;
using System.Linq;
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

        internal async Task<Room> Update(DatabaseChat New)
        {
            var Current = (await Database.Client.GetAsync($"chat/{Topic}/data")).ResultAs<DatabaseChat>();
            if (Current.Tags == null)
            {
                Current.Tags = new string[0];
            }

            foreach (var Tag in New.Tags)
            {
                if (!Current.Tags.Contains(Tag))
                {
                    await Database.Client.SetAsync($"tags/{Tag}/{Topic}", true);
                }
            }

            foreach (var Tag in Current.Tags)
            {
                if (!New.Tags.Contains(Tag))
                {
                    await Database.Client.DeleteAsync($"tags/{Tag}/{Topic}");
                }
            }

            await Database.Client.SetAsync($"chat/{Topic}/data", New);

            return this;
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
