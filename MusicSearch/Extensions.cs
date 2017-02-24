using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MusicSearch
{
    static class Extensions
    {
        internal static StringBuilder UcWords(this object In)
        {
            var Output = new StringBuilder();
            var Pieces = In.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var piece in Pieces)
            {
                var Chars = piece.ToCharArray();
                Chars[0] = char.ToUpper(Chars[0]);
                Output.Append(' ');
                Output.Append(Chars);
            }

            return Output;
        }

        public static async Task<string> WebResponse(this string Url, WebHeaderCollection Headers = null)
        {
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        var Request = WebRequest.Create(Url);
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
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex);
            }

            return string.Empty;
        }
    }
}
