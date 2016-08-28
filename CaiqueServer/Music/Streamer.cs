using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CaiqueServer.Music
{
    class Streamer
    {
        private static IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
        private const string IcecastPass = "caiquev6";
        private static string IcecastAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"source:{IcecastPass}"));

        private static ConcurrentDictionary<long, Streamer> Streamers = new ConcurrentDictionary<long, Streamer>();
        private static CancellationTokenSource Stop = new CancellationTokenSource();
        private static ConcurrentBag<ManualResetEvent> ShutdownCompleted = new ConcurrentBag<ManualResetEvent>();

        internal static Streamer Get(long Id)
        {
            return Streamers.GetOrAdd(Id, delegate (long StreamId)
            {
                return new Streamer(StreamId);
            });
        }

        internal static void Shutdown()
        {
            Stop.Cancel();
            WaitHandle.WaitAll(ShutdownCompleted.ToArray());
        }

        internal Songdata Song;
        private ConcurrentQueue<Songdata> Queue = new ConcurrentQueue<Songdata>();
        internal const int MaxQueued = 16;

        internal CancellationTokenSource Skip = new CancellationTokenSource();
        private TaskCompletionSource<bool> EmptyQueue;

        private string Link;
        private string PlaylistFile
        {
            get
            {
                return $"{Link}.playlist";
            }
        }

        internal Streamer(long Id)
        {
            Link = "stream" + (++Id).ToString();

            if (File.Exists(PlaylistFile))
            {
                using (var Reader = new BinaryReader(File.Open(PlaylistFile, FileMode.Open)))
                {
                    var Count = Reader.ReadInt32();
                    for (int i = 0; i < Count; i++)
                    {
                        Queue.Enqueue(new Songdata
                        {
                            FullName = Reader.ReadString(),
                            Url = Reader.ReadString(),
                            Type = (SongType)Reader.ReadInt32(),
                            Thumbnail = Reader.ReadString()
                        });
                    }
                }
            }

            var Reset = new ManualResetEvent(false);
            ShutdownCompleted.Add(Reset);
            BackgroundStream().ContinueWith(delegate
            {
                Reset.Set();
            });
        }

        internal void Enqueue(Songdata Song)
        {
            Queue.Enqueue(Song);
            EmptyQueue?.TrySetResult(true);
        }

        private async Task BackgroundStream()
        {
            while (!Stop.IsCancellationRequested)
            {
                try
                {
                    EmptyQueue = new TaskCompletionSource<bool>();
                    using (EmptyQueue.Task)
                    {
                        await StreamUntilQueueEmpty();
                        await EmptyQueue.Task;
                    }
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Link + " " + Ex.ToString());
                }
            }
        }

        private async Task StreamUntilQueueEmpty()
        {
            using (var Client = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                await Client.ConnectAsync(EndPoint);
                await Client.SendAsync(Encoding.ASCII.GetBytes("SOURCE /" + Link + @" ICE/1.0
content-type: audio/aac
Authorization: Basic " + IcecastAuth + @"
ice-name: Radio
ice-description: Stream
ice-url: http://www.google.com
ice-genre: All
ice-private: 0
ice-public: 1
ice-audio-info: ice-samplerate=44100;ice-bitrate=128;ice-channels=2

"));

                using (var Output = new NetworkStream(Client))
                {
                    var ProcessStartInfo = new ProcessStartInfo
                    {
                        FileName = "Includes/ffmpeg",
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };

                    while (!Stop.IsCancellationRequested && Queue.TryDequeue(out Song))
                    {
                        UpdateMetadataAndSave();

                        //ProcessStartInfo.Arguments = $"-re -i includes/whitenoise.m4a -vn -codec:a copy -f adts -v quiet pipe:1";
                        ProcessStartInfo.Arguments = $"-re -i \"{Song.StreamUrl}\" -vn -codec:a aac -b:a 128k -f adts -ac 2 -ar 48k -v quiet pipe:1";

                        using (var Ffmpeg = Process.Start(ProcessStartInfo))
                        using (var Input = Ffmpeg.StandardOutput.BaseStream)
                        using (var Cancel = CancellationTokenSource.CreateLinkedTokenSource(Skip.Token, Stop.Token))
                        {
                            try
                            {
                                await Input.CopyToAsync(Output, 81920, Cancel.Token);
                            }
                            catch (TaskCanceledException)
                            {
                                Skip = new CancellationTokenSource();
                            }
                        }
                    }
                }
            }
        }

        private async void UpdateMetadataAndSave()
        {
            try
            {
                using (var Writer = new BinaryWriter(File.Open(PlaylistFile, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    var List = Queue.ToList();
                    List.Insert(0, Song);
                    List = List.Where(x => x.FullName != null).ToList();

                    Writer.Write(List.Count);
                    foreach (var Item in List)
                    {
                        Writer.Write(Item.FullName);
                        Writer.Write(Item.Url);
                        Writer.Write((int)Item.Type);
                        Writer.Write(Item.Thumbnail ?? string.Empty);
                    }
                }

                using (var Client = new Socket(SocketType.Stream, ProtocolType.Tcp))
                {
                    await Client.ConnectAsync(EndPoint);
                    await Client.SendAsync(Encoding.ASCII.GetBytes($"GET /admin/metadata?pass={IcecastPass}&mode=updinfo&mount=/{Link}&song={HttpUtility.UrlEncode(Song.FullName)}" + @" HTTP/1.0
Authorization: Basic " + IcecastAuth + @"
User-Agent: (Mozilla Compatible)

"));
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }
        }
    }
}
