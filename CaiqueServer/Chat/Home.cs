using CaiqueServer.Firebase.Json;
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

        internal static async Task<IOrderedEnumerable<KeyValuePair<string, DatabaseChat>>> ByTags(string[] Tags)
        {
            List<string> ChatKeys = null;
            foreach (var Tag in Tags)
            {
                if (ChatKeys == null)
                {
                    ChatKeys = (await Firebase.Database.Client.GetAsync($"tags/{Tag}")).ResultAs<Dictionary<string, bool>>().Keys.ToList();
                }
                else
                {
                    ChatKeys = (await Firebase.Database.Client.GetAsync($"tags/{Tag}")).ResultAs<Dictionary<string, bool>>().Keys.Where(x => ChatKeys.Contains(x)).ToList();
                }
            }

            var Chats = new Dictionary<string, DatabaseChat>();
            foreach (var ChatKey in ChatKeys)
            {
                Chats.Add(ChatKey, (await Firebase.Database.Client.GetAsync($"chat/{ChatKey}/data")).ResultAs<DatabaseChat>());
            }

            return Chats.OrderBy(x => x.Value.Tags.Length);
        }
    }
}
