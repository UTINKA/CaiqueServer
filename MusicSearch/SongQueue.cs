using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicSearch
{
    public class SongQueue
    {
        public int MaxQueued = 25;

        public Song Playing
        {
            get
            {
                return mPlaying;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return !string.IsNullOrEmpty(mPlaying.Url);
            }
        }

        public bool CanAdd
        {
            get
            {
                return Queue.Count <= MaxQueued;
            }
        }

        public int Count
        {
            get
            {
                return Queue.Count;
            }
        }

        private Song mPlaying;
        private ConcurrentQueue<Song> Queue = new ConcurrentQueue<Song>();

        public int Enqueue(Song Song)
        {
            if (CanAdd)
            {
                Queue.Enqueue(Song);
                return Queue.Count;
            }

            return 0;
        }

        public bool Next()
        {
            Invalidate();

            if (!Queue.TryDequeue(out mPlaying))
            {
                return false;
            }

            return true;
        }

        public void Invalidate()
        {
            mPlaying = default(Song);
        }

        public int Repeat(int Count)
        {
            if (IsPlaying)
            {
                var Songs = ToArray();
                if (Count + Songs.Length > MaxQueued)
                {
                    Count = MaxQueued - Songs.Length;
                }

                var NewQueue = new ConcurrentQueue<Song>();

                for (int i = 0; i < Count; i++)
                {
                    NewQueue.Enqueue(Playing);
                }

                foreach (var Song in Queue)
                {
                    NewQueue.Enqueue(Song);
                }

                Queue = NewQueue;
                return Count;
            }

            return 0;
        }

        internal bool TryPush(int Place, int ToPlace, out Song Pushed)
        {
            Pushed = default(Song);

            var NewQueue = new ConcurrentQueue<Song>();
            var Songs = ToList();
            if (Place > 0 && ToPlace > 0 && Place != ToPlace && Songs.Count >= Place && Songs.Count >= ToPlace)
            {
                Pushed = Songs[Place - 1];
                Songs.Remove(Pushed);
                Songs.Insert(ToPlace - 1, Pushed);

                foreach (var Song in Songs)
                {
                    NewQueue.Enqueue(Song);
                }

                Queue = NewQueue;
                return true;
            }

            return false;
        }

        public bool TryRemove(ushort Index, out Song Song)
        {
            Song = default(Song);
            var List = ToList();
            if (List.Count <= Index)
            {
                return false;
            }

            Song = List[Index];
            List.RemoveAt(Index);
            Queue = new ConcurrentQueue<Song>(List);

            return true;
        }

        public Song[] ToArray()
            => Queue.ToArray();

        public List<Song> ToList()
            => Queue.ToList();

        public Task<string> StreamUrl(bool AllFormats = true)
            => SongRequest.StreamUrl(Playing, AllFormats);
    }
}
