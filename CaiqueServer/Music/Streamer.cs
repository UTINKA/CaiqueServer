using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CaiqueServer.Music
{
    class Streamer
    {
        private const string IcecastPass = "caiquev6";
        private static ConcurrentDictionary<string, Streamer> Streamers = new ConcurrentDictionary<string, Streamer>();
        private static CancellationTokenSource Stop = new CancellationTokenSource();
        private static ConcurrentBag<ManualResetEvent> ShutdownCompleted = new ConcurrentBag<ManualResetEvent>();

        internal static Streamer Get(string Chat)
        {
            return Streamers.GetOrAdd(Chat, delegate (string Id)
            {
                return new Streamer(Id);
            });
        }

        internal static bool TryGetSong(string Chat, out SongData Out)
        {
            Streamer Streamer;
            if (Streamers.TryGetValue(Chat, out Streamer) && Streamer.Song.Url != null)
            {
                Out = Streamers[Chat].Song;
                return true;
            }

            Out = new SongData();
            return false;
        }

        internal static string Serialize(string Chat)
        {
            Streamer Streamer;
            if (Streamers.TryGetValue(Chat, out Streamer))
            {
                return JsonConvert.SerializeObject(Streamer.Queue);
            }

            return string.Empty;
        }

        internal static void Shutdown()
        {
            Stop.Cancel();
            Parallel.ForEach(Streamers, (KVP, s) =>
            {
                KVP.Value.Skip();
            });

            var Shutdown = ShutdownCompleted.ToArray();
            if (Shutdown.Length != 0)
            {
                WaitHandle.WaitAll(Shutdown);
            }
        }

        internal Process Ffmpeg { get; private set; }
        internal SongData Song;
        private ConcurrentQueue<SongData> Queue = new ConcurrentQueue<SongData>();
        private const int MaxQueued = 16;

        private TaskCompletionSource<bool> ProcessWaiter;
        private SemaphoreSlim WaitAdd = new SemaphoreSlim(0);

        private string Id;

        internal Streamer(string Chat)
        {
            Id = Chat;

            var Reset = new ManualResetEvent(false);
            ShutdownCompleted.Add(Reset);
            BackgroundStream().ContinueWith(delegate
            {
                Reset.Set();
            });
        }

        private async Task BackgroundStream()
        {
            Console.WriteLine("Started background stream for " + Id);
            DataReceivedEventHandler Handler = null;
            var ProcessStartInfo = new ProcessStartInfo
            {
                FileName = "Includes/ffmpeg",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true
            };

            while (!Stop.IsCancellationRequested)
            {
                try
                {
                    await WaitAdd.WaitAsync(Stop.Token);
                    Queue.TryDequeue(out Song);

                    using (Ffmpeg = new Process())
                    {
                        ProcessStartInfo.Arguments = $"-re -i \"{Song.StreamUrl}\" -vn -content_type audio/aac -f adts ";
                        if (Song.Type == SongType.YouTube || Song.Type == SongType.Uploaded)
                        {
                            ProcessStartInfo.Arguments += "-c:a copy ";
                        }
                        else
                        {
                            ProcessStartInfo.Arguments += $"-c:a aac -b:a 96k -ac 2 -ar 48k ";
                        }
                        ProcessStartInfo.Arguments += $"icecast://source:{IcecastPass}@localhost:80/{Id}";

                        ProcessWaiter = new TaskCompletionSource<bool>();

                        Ffmpeg.StartInfo = ProcessStartInfo;
                        Ffmpeg.EnableRaisingEvents = true;
                        Ffmpeg.Exited += (s, e) =>
                        {
                            ProcessWaiter.TrySetResult(true);
                        };

                        Handler = async (s, e) =>
                        {
                            Console.WriteLine(e.Data ?? "");
                            if (e.Data?.StartsWith("size=") ?? false)
                            {
                                Ffmpeg.ErrorDataReceived -= Handler;
                                await Task.Delay(250).ContinueWith(delegate
                                {
                                    Chat.Home.ById(Id).Distribute(new Cloud.Json.Event
                                    {
                                        Chat = Id,
                                        Type = "play",
                                        Text = Song.Title,
                                        Sender = Song.Adder
                                    });
                                });
                            }
                        };

                        Ffmpeg.ErrorDataReceived += Handler;

                        Ffmpeg.Start();
                        Ffmpeg.BeginErrorReadLine();
                        Ffmpeg.PriorityClass = ProcessPriorityClass.BelowNormal;
                        await ProcessWaiter.Task;

                        Ffmpeg.CancelErrorRead();
                        await Ffmpeg.StandardInput.WriteAsync("q");
                    }
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.ToString());
                }
                finally
                {
                    Song.Url = null;
                    Console.WriteLine("Stopped Playing");
                }
            }
        }

        internal bool Enqueue(string Song, string Adder)
        {
            var Results = SongData.Search(Song, 1);
            if (Results.Count == 0)
            {
                return false;
            }

            return Enqueue(Results[0], Adder);
        }

        internal bool Enqueue(SongData Song, string Adder)
        {
            if (Queue.Count >= MaxQueued)
            {
                return false;
            }

            Queue.Enqueue(Song);
            WaitAdd.Release();
            return true;
        }

        internal bool Push(int Place, int ToPlace)
        {
            var NewQueue = new ConcurrentQueue<SongData>();
            var Songs = Queue.ToList();
            if (Place > 0 && ToPlace > 0 && Songs.Count >= Place && Songs.Count >= ToPlace)
            {
                var Pushed = Songs[Place - 1];
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
        
        internal bool Remove(int ToRemove)
        {
            var NewQueue = new ConcurrentQueue<SongData>();
            var Songs = Queue.ToList();
            if (ToRemove > 0 && Songs.Count >= ToRemove)
            {
                Songs.Remove(Songs[ToRemove - 1]);
                foreach (var Song in Songs)
                {
                    NewQueue.Enqueue(Song);
                }

                Queue = NewQueue;
                return true;
            }

            return false;
        }

        internal void Skip()
        {
            ProcessWaiter?.TrySetResult(false);
        }
    }
}
