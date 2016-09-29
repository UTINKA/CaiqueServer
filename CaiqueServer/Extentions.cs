using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CaiqueServer
{
    static class Extentions
    {
        public static string SafePath(this string In)
        {
            return In.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        public static string Md5(this string In)
        {
            var InBytes = Encoding.Unicode.GetBytes(In);

            var Result = new StringBuilder();
            byte[] Hash;

            using (var Md5 = MD5.Create())
            {
                Hash = Md5.ComputeHash(InBytes);
            }

            for (int i = 0; i < Hash.Length; i++)
            {
                Result.Append(Hash[i].ToString("x2"));
            }

            return Result.ToString();
        }

        public static string ToJson(this object In)
        {
            return JsonConvert.SerializeObject(In, Formatting.None, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });
        }

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

        public static bool IsValidUrl(this string Text)
        {
            Uri WebRes;
            return Uri.TryCreate(Text, UriKind.Absolute, out WebRes);
        }

        public static async Task<string> WebResponse(this string Url, WebHeaderCollection Headers = null)
        {
            for (int i = 0; i < 10; i++)
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
    }
}
