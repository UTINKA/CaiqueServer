using Newtonsoft.Json;
using MusicSearch;
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

        internal static bool TryGetSong(string Chat, out Song Out)
        {
            Streamer Streamer;
            if (Streamers.TryGetValue(Chat, out Streamer) && Streamer.Queue.IsPlaying)
            {
                Out = Streamer.Queue.Playing;
                return true;
            }

            Out = new Song();
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
        internal SongQueue Queue = new SongQueue();

        private TaskCompletionSource<bool> ProcessWaiter;
        private SemaphoreSlim WaitAdd = new SemaphoreSlim(0);

        private string Id;

        internal Streamer(string Chat)
        {
            Queue.MaxQueued = 16;
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
                    Queue.Next();

                    using (Ffmpeg = new Process())
                    {
                        ProcessStartInfo.Arguments = $"-re -i \"{await Queue.StreamUrl(false)}\" -vn -content_type audio/aac -f adts ";
                        if (Queue.Playing.Type == SongType.YouTube || Queue.Playing.Type == SongType.Uploaded)
                        {
                            ProcessStartInfo.Arguments += "-c:a copy ";
                        }
                        else
                        {
                            ProcessStartInfo.Arguments += $"-c:a aac -b:a 128k -ac 2 -ar 48k ";
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
                                        Text = Queue.Playing.Title,
                                        Sender = Queue.Playing.Adder
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
                    Queue.Invalidate();
                    Console.WriteLine("Stopped Playing");
                }
            }
        }

        internal async Task<bool> Enqueue(string Song, string Adder)
        {
            var Results = await SongRequest.Search(Song, false);
            if (Results.Count == 0)
            {
                return false;
            }

            return Enqueue(Results[0], Adder);
        }

        internal bool Enqueue(Song Song, string Adder)
        {
            Song.Adder = Adder;
            if (Queue.Enqueue(Song) != 0)
            {
                WaitAdd.Release();
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
