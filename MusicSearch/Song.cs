using Newtonsoft.Json;

namespace MusicSearch
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct Song
    {
        [JsonProperty("name", Required = Required.Always)]
        public string FullName;

        [JsonProperty("desc", Required = Required.Always)]
        public string Desc;

        [JsonProperty("url", Required = Required.Always)]
        public string Url;

        [JsonProperty("adder", Required = Required.Default)]
        public string Adder;

        [JsonProperty("type", Required = Required.Always)]
        public SongType Type;

        [JsonProperty("tn", Required = Required.Default)]
        public string Thumbnail;
        
        public string GetThumbnail(string Replacement)
            => Thumbnail != null && Thumbnail != string.Empty ? Thumbnail : Replacement;

        public string Title
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
    }
}
