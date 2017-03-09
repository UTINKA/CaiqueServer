using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
//using System.Net.Sockets;
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

        /*public static Task ConnectAsync(this Socket Connection, IPEndPoint EndPoint)
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
        }*/

        public static bool IsValidUrl(this string Text)
        {
            Uri WebRes;
            return Uri.TryCreate(Text, UriKind.Absolute, out WebRes);
        }

        public static async Task<string> WebResponse(this string Url, WebHeaderCollection Headers = null)
        {
            try
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

                        return await new StreamReader(
                                (await Request.GetResponseAsync())
                                .GetResponseStream()
                            )
                            .ReadToEndAsync();
                    }
                    catch (Exception Ex2)
                    {
                        if (i == 4)
                        {
                            throw Ex2;
                        }
                    }

                    await Task.Delay(i * 500);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex);
            }

            return string.Empty;
        }

        public static StringBuilder UcWords(this object theString)
        {
            var output = new StringBuilder();
            var pieces = theString.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var piece in pieces)
            {
                var theChars = piece.ToCharArray();
                theChars[0] = char.ToUpper(theChars[0]);
                output.Append(' ');
                output.Append(new string(theChars));
            }

            return output;
        }
    }
}
