using System.Collections.Concurrent;

namespace CaiqueServer.Chat
{
    class Home
    {
        private static ConcurrentDictionary<int, Room> Rooms = new ConcurrentDictionary<int, Room>();

        internal static Room ById(int Chat)
        {
            return Rooms.GetOrAdd(Chat, delegate (int Id)
            {
                return new Room(Id);
            });
        }
    }
}
