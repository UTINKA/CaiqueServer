using System.Collections.Concurrent;

namespace CaiqueServer.Chat
{
    class Home
    {
        private static ConcurrentDictionary<string, Room> Rooms = new ConcurrentDictionary<string, Room>();

        internal static Room ById(string Chat)
        {
            return Rooms.GetOrAdd(Chat, delegate (string Id)
            {
                return new Room(Id);
            });
        }
    }
}
