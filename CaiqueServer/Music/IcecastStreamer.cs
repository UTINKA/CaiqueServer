using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CaiqueServer.Music
{
    class IcecastStreamer
    {
        private static IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
        private const string IcecastPass = "caiquesource";
        private static string IcecastAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"source:{IcecastPass}"));

        internal Songdata Song;
        internal ConcurrentQueue<Songdata> Queue = new ConcurrentQueue<Songdata>();
        internal const int MaxQueued = 16;

        internal CancellationTokenSource Skip = new CancellationTokenSource();
        internal CancellationTokenSource CloseRequester = new CancellationTokenSource();

        private string Link;

        internal IcecastStreamer(long Id)
        {
            Link = "/stream" + Id.ToString();
        }

        internal async void StreamLoop()
        {
            while (!CloseRequester.IsCancellationRequested)
            {
                try
                {
                    await Stream();
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.ToString());
                }
            }
        }

        internal async Task Stream()
        {
            using (var Client = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                await Client.ConnectAsync(EndPoint);
                await Client.SendAsync(Encoding.ASCII.GetBytes("SOURCE " + Link + @" ICE/1.0
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
                    while (!CloseRequester.IsCancellationRequested)
                    {
                        if (!Queue.TryDequeue(out Song))
                        {
                            /*Song = new Songdata
                            {
                                FullName = string.Empty,
                                Url = "Includes/whitenoise.wav",
                                Type = SongType.Local
                            };*/

                            //Song = Songdata.Search("Nano Gallows Bell")[0];

                            await Task.Delay(3000);
                            continue;
                        }

                        UpdateMetadata();

                        using (var Ffmpeg = Process.Start(new ProcessStartInfo
                        {
                            FileName = "Includes/ffmpeg",
                            Arguments = $"-re -i \"{Song.StreamUrl}\" -vn -codec:a aac -b:a 128k -f adts -ac 2 -ar 48k -v quiet pipe:1",
                            UseShellExecute = false,
                            RedirectStandardOutput = true
                        }))
                        using (var Input = Ffmpeg.StandardOutput.BaseStream)
                        {
                            try
                            {
                                await Input.CopyToAsync(Output, 81920, Skip.Token);
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

        internal async void UpdateMetadata()
        {
            try
            {
                await Task.Yield();

                using (var Client = new Socket(SocketType.Stream, ProtocolType.Tcp))
                {
                    await Client.ConnectAsync(EndPoint);
                    await Client.SendAsync(Encoding.ASCII.GetBytes($"GET /admin/metadata?pass={IcecastPass}&mode=updinfo&mount={Link}&song={HttpUtility.UrlEncode(Song.FullName)}" + @" HTTP/1.0
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
