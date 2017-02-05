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

        internal static async Task<Dictionary<string, DatabaseChat>> ByTags(string[] Tags)
        {
            List<string> ChatKeys = null;
            var Chats = new Dictionary<string, DatabaseChat>();

            foreach (var Tag in Tags)
            {
                var TagResults = (await Cloud.Database.Client.GetAsync($"tags/{Tag}")).ResultAs<Dictionary<string, bool>>();
                if (TagResults == null)
                {
                    return Chats;
                }

                if (ChatKeys == null)
                {
                    ChatKeys = TagResults.Keys.ToList();
                }
                else
                {
                    ChatKeys = TagResults.Keys.Where(x => ChatKeys.Contains(x)).ToList();
                }
            }

            foreach (var ChatKey in ChatKeys)
            {
                Chats.Add(ChatKey, (await Cloud.Database.Client.GetAsync($"chat/{ChatKey}/data")).ResultAs<DatabaseChat>());
            }

            return Chats.OrderBy(x => x.Value.Tags.Length).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
