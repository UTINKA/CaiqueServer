using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using VideoLibrary;

namespace CaiqueServer.Music
{
    [JsonObject(MemberSerialization.OptIn)]
    struct SongData
    {
        private static YouTubeService YT = new YouTubeService(new BaseClientService.Initializer
        {
            ApiKey = "AIzaSyAVrXiAHfLEbQbNJP80zbTuW2jL0wuEigQ"
        });

        private static string SoundCloud = "5c28ed4e5aef8098723bcd665d09041d";
        private static string MusicDir = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\Music\\";
        internal static DirectoryInfo Dir = new DirectoryInfo(MusicDir);
        private static readonly Regex YoutubeVideoRegex = new Regex(@"youtu(?:\.be|be\.com)/(?:(.*)v(/|=)|(.*/)?)([a-zA-Z0-9-_]+)", RegexOptions.IgnoreCase);
        
        [JsonProperty("name", Required = Required.Always)]
        internal string FullName;

        [JsonProperty("desc", Required = Required.Always)]
        internal string Description;

        [JsonProperty("url", Required = Required.Always)]
        internal string Url;

        [JsonProperty("adder", Required = Required.Always)]
        internal string Adder;

        [JsonProperty("type", Required = Required.Always)]
        internal SongType Type;

        [JsonProperty("tn", Required = Required.Default)]
        internal string Thumbnail;

        internal string Title
        {
            get
            {
                if (FullName.Length < 192)
                {
                    return FullName;
                }

                return FullName.Substring(0, 192);
            }
        }

        internal async Task<string> StreamUrl()
        {
            if (Type == SongType.YouTube)
            {
                var Videos = YouTube.Default.GetAllVideos(Url);
                YouTubeVideo MaxVid = null;
                foreach (var Vid in Videos)
                {
                    if (MaxVid == null || Vid.AudioBitrate >= MaxVid.AudioBitrate)
                    {
                        MaxVid = Vid;
                    }
                }

                if (MaxVid == null)
                {
                    return string.Empty;
                }

                return await MaxVid.GetUriAsync();
            }
            else if (Type == SongType.SoundCloud)
            {
                var SC = await ($"http://api.soundcloud.com/resolve?url={Url}&client_id={SoundCloud}").WebResponse();
                if (SC != string.Empty && SC.StartsWith("{\"kind\":\"track\""))
                {
                    return $"{JObject.Parse(SC)["stream_url"]}?client_id={SoundCloud}";
                }
            }

            return Url;
        }

        internal static async Task<List<SongData>> Search(object ToSearch, string Adder, bool ReturnAtOnce = false)
        {
            var Query = ((string)ToSearch).Trim();
            var Results = new List<SongData>();

            var Match = YoutubeVideoRegex.Match(Query);
            if (Match.Success)
            {
                var Search = YT.Videos.List("snippet");
                Search.Id = Match.Groups[4].Value;
                var Videos = await Search.ExecuteAsync();
                var Result = Videos.Items.First();
                if (Result != null)
                {
                    Results.Add(new SongData
                    {
                        FullName = Result.Snippet.Title,
                        Description = "YouTube | " + Result.Snippet.Description,
                        Url = $"http://www.youtube.com/watch?v={Search.Id}",
                        Adder = Adder,
                        Type = SongType.YouTube,
                        Thumbnail = Result.Snippet.Thumbnails.Maxres?.Url ?? Result.Snippet.Thumbnails.Default__?.Url
                    });
                }

            }
            else if (Regex.IsMatch(Query, "(.*)(soundcloud.com|snd.sc)(.*)"))
            {
                var SC = await ($"http://api.soundcloud.com/resolve?url={Query}&client_id={SoundCloud}").WebResponse();
                if (SC != string.Empty && SC.StartsWith("{\"kind\":\"track\""))
                {
                    Results.Add(SoundCloudParse(JToken.Parse(SC), Adder));
                }
            }
            else if (Query.IsValidUrl())
            {
                Results.Add(new SongData
                {
                    FullName = Query,
                    Description = string.Empty,
                    Url = Query,
                    Adder = Adder,
                    Type = SongType.Remote
                });
            }

            if (Query.Length >= 3)
            {
                var Range = Dir.GetFiles()
                    .Where(x => x.Name.Length >= Query.Length && x.Name.ToLower().Contains(Query.ToLower()) && !x.Attributes.HasFlag(FileAttributes.System))
                    .OrderBy(x => x.Name)
                    .Select(x => new SongData
                    {
                        FullName = x.Name,
                        Description = $"Local {x.Extension.Substring(1).ToUpper()} File",
                        Url = x.FullName,
                        Adder = Adder,
                        Type = SongType.Local
                    });

                if (Range.Count() > 3)
                {
                    Range = Range.Take(3);
                }

                Results.AddRange(Range);
            }

            if (!ReturnAtOnce || Results.Count == 0)
            {
                var SC = ($"http://api.soundcloud.com/tracks/?client_id={SoundCloud}&q={HttpUtility.UrlEncode(Query)}").WebResponse();

                var ListRequest = YT.Search.List("snippet");
                ListRequest.Q = Query;
                ListRequest.MaxResults = 3;
                ListRequest.Type = "video";
                foreach (var Result in (await ListRequest.ExecuteAsync()).Items)
                {
                    Results.Add(new SongData
                    {
                        FullName = Result.Snippet.Title,
                        Description = "YouTube | " + Result.Snippet.Description,
                        Url = $"http://www.youtube.com/watch?v={Result.Id.VideoId}",
                        Adder = Adder,
                        Type = SongType.YouTube,
                        Thumbnail = Result.Snippet.Thumbnails.Maxres?.Url ?? Result.Snippet.Thumbnails.Default__?.Url
                    });
                }

                var SCResponse = JArray.Parse(await SC);
                int i = 0;
                foreach (var Response in SCResponse)
                {
                    if (++i > 3)
                    {
                        break;
                    }

                    Results.Add(SoundCloudParse(Response, Adder));
                }
            }

            return Results;
        }

        private static SongData SoundCloudParse(JToken Response, string Adder)
        {
            var Desc = Response["description"].ToString().Replace("\n", " ");
            if (Desc.Length == 0)
            {
                Desc = Response["genre"].UcWords().ToString();
            }

            var Thumb = Response["artwork_url"].ToString();
            if (Thumb == string.Empty)
            {
                Thumb = "http://i.imgur.com/eRaxycY.png";
            }

            return new SongData
            {
                FullName = Response["title"].ToString(),
                Description = $"SoundCloud | {Desc.Trim()}",
                Url = Response["uri"].ToString(),
                Adder = Adder,
                Type = SongType.SoundCloud,
                Thumbnail = Thumb
            };
        }
    }
}
