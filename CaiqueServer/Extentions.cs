using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CaiqueServer
{
    static class Extentions
    {
        public static Task ConnectAsync(this Socket Connection, IPEndPoint EndPoint)
        {
            return Task.Factory.FromAsync(Connection.BeginConnect(EndPoint, null, null), Connection.EndConnect);
        }

        public static Task SendAsync(this Socket Connection, byte[] Data)
        {
            return Task.Factory.FromAsync(Connection.BeginSend(Data, 0, Data.Length, SocketFlags.None, null, null), Connection.EndSend);
        }

        public static Task DisconnectAsync(this Socket Connection)
        {
            return Task.Factory.FromAsync(Connection.BeginDisconnect(false, null, null), Connection.EndDisconnect);
        }

        public static string MaxSubstring(this string Source, int MaxLength, string Add = "")
        {
            if (Source.Length < MaxLength)
            {
                return Source;
            }

            return Source.Substring(0, MaxLength - Add.Length) + Add;
        }

        public static string Substring(this string Source, string Trim)
        {
            return Source.Substring(Trim.Length);
        }

        public static bool IsValidUrl(this string Text)
        {
            Uri WebRes;
            return Uri.TryCreate(Text, UriKind.Absolute, out WebRes);
        }

        public static async Task<string> WebResponse(this string Url, WebHeaderCollection Headers = null)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    WebRequest Request = WebRequest.Create(Url);
                    if (Headers != null)
                    {
                        Request.Headers = Headers;
                    }

                    return await new StreamReader(Request.GetResponse().GetResponseStream()).ReadToEndAsync();
                }
                catch
                {
                }
            }

            return string.Empty;
        }

        public static bool TryRemove<P, Q>(this ConcurrentDictionary<P, Q> In, P Key)
        {
            Q Out;
            return In.TryRemove(Key, out Out);
        }
    }
}
