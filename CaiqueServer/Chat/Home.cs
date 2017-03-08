using CaiqueServer.Cloud.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaiqueServer.Chat
{
    class Home
    {
        private struct RoomScore
        {
            internal Room Room;
            internal int Score;
        }

        private static ConcurrentDictionary<string, Room> Rooms = new ConcurrentDictionary<string, Room>();

        internal static Room ById(string Chat)
        {
            return Rooms.GetOrAdd(Chat, delegate (string Id)
            {
                return new Room(Id);
            });
        }

        internal static async Task<string[]> ByTags(string[] Tags)
        {
            List<string> Chats = null;
            var Relevancy = new Dictionary<string, int>();

            foreach (var Tag in Tags)
            {
                var TagResults = (await Cloud.Database.Client.GetAsync($"tags/{Tag}")).ResultAs<Dictionary<string, bool>>();
                if (TagResults == null)
                {
                    return new string[0];
                }

                if (Chats == null)
                {
                    Chats = TagResults.Keys.ToList();
                }
                else
                {
                    Chats = TagResults.Keys.Where(x => Chats.Contains(x)).ToList();
                }
            }

            foreach (var Chat in Chats)
            {
                var TagsChatRes = await Cloud.Database.Client.GetAsync($"chat/{Chat}/data/tags");
                var TagsChat = TagsChatRes.ResultAs<object[]>();
                Relevancy.Add(Chat, TagsChat.Length);
            }

            return Chats.OrderBy(x => Relevancy[x]).ToArray();
        }
    }
}
