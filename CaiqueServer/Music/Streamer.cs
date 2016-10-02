using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CaiqueServer.Music
{
    class Streamer
    {
        private struct AddedSong
        {
            internal string Adder;
            internal Songdata Data;
        }

        private const string IcecastPass = "caiquev6";
        private static ConcurrentDictionary<string, Streamer> Streamers = new ConcurrentDictionary<string, Streamer>();
        private static CancellationTokenSource Stop = new CancellationTokenSource();
        private static ConcurrentBag<ManualResetEvent> ShutdownCompleted = new ConcurrentBag<ManualResetEvent>();

        private static int AtomicPort = (ushort.MaxValue + 1) / 8;
        private static ConcurrentQueue<ushort> ReusePorts = new ConcurrentQueue<ushort>();

        internal static Streamer Get(string Chat)
        {
            return Streamers.GetOrAdd(Chat, delegate (string Id)
            {
                return new Streamer(Id);
            });
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

        private AddedSong _Song;
        internal Songdata Song
        {
            get
            {
                return _Song.Data;
            }
        }
        private ConcurrentQueue<AddedSong> Queue = new ConcurrentQueue<AddedSong>();
        private const int MaxQueued = 16;
        
        internal TaskCompletionSource<bool> Process;
        private TaskCompletionSource<bool> WaitAdd;

        private string Id;
        private ushort Port;

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

        internal bool Enqueue(string Song, string Adder)
        {
            var Results = Songdata.Search(Song, 1);
            if (Results.Count == 0)
            {
                return false;
            }

            return Enqueue(Results[0], Adder);
        }

        internal bool Enqueue(Songdata Song, string Adder)
        {
            if (Queue.Count >= MaxQueued)
            {
                return false;
            }

            Queue.Enqueue(new AddedSong { Data = Song, Adder = Adder });
            WaitAdd?.TrySetResult(true);
            return true;
        }

        internal void Skip()
        {
            Process?.TrySetCanceled();
        }

        private async Task BackgroundStream()
        {
            Console.WriteLine("Started background stream for " + Id);

            while (true)
            {
                WaitAdd = new TaskCompletionSource<bool>();

                if (!ReusePorts.TryDequeue(out Port))
                {
                    Port = (ushort)Interlocked.Increment(ref AtomicPort);
                }

                try
                {
                    await StreamUntilQueueEmpty();
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Id + " " + Ex.ToString());
                }

                ReusePorts.Enqueue(Port);

                try
                {
                    await WaitAdd.Task;
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Id + " " + Ex.ToString());
                }
            }
        }

        private async Task StreamUntilQueueEmpty()
        {
            var ProcessStartInfo = new ProcessStartInfo
            {
                FileName = "Includes/ffmpeg",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            while (!Stop.IsCancellationRequested && Queue.TryDequeue(out _Song))
            {
                //ProcessStartInfo.Arguments = $"-re -i \"{Song.StreamUrl}\" -vn -content_type audio/aac -f adts ";
                ProcessStartInfo.Arguments = $"-re -i \"{Song.StreamUrl}\" -vn -f adts ";
                if (Song.Type == SongType.YouTube)
                {
                    ProcessStartInfo.Arguments += "-c:a copy ";
                }
                else
                {
                    ProcessStartInfo.Arguments += $"-c:a aac -b:a 96k -ac 2 -ar 48k ";
                }

                //ProcessStartInfo.Arguments += $"icecast://source:{IcecastPass}@localhost:80/{Id}";
                //ProcessStartInfo.Arguments += $"-f rtsp rtsp://127.0.0.1:1935/live/myStream.sdp";
                ProcessStartInfo.Arguments += $"udp://127.0.0.1:{Port}";

                using (var Ffmpeg = new Process())
                {
                    Process = new TaskCompletionSource<bool>();

                    try
                    {
                        Ffmpeg.StartInfo = ProcessStartInfo;
                        Ffmpeg.EnableRaisingEvents = true;
                        Ffmpeg.Exited += delegate
                        {
                            Process.TrySetResult(true);
                        };

                        Stop.Token.Register(delegate
                        {
                            Process.TrySetCanceled();
                        });

                        Ffmpeg.Start();
                        Ffmpeg.PriorityClass = ProcessPriorityClass.BelowNormal;

                        Chat.Home.ById(Id).Distribute(new Firebase.Json.Event
                        {
                            Chat = Id,
                            Type = "play",
                            Text = Song.Title,
                            Sender = _Song.Adder,
                            Attachment = Port.ToString()
                        });

                        await Process.Task;
                    }
                    catch (TaskCanceledException)
                    {
                    }

                    try
                    {
                        await Ffmpeg.StandardInput.WriteAsync("q");
                    }
                    catch { }
                }
            }
        }
    }
}
