using Newtonsoft.Json;

namespace MusicSearch
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct Song
    {
        public static int MaxLength = 100;

        [JsonProperty("name", Required = Required.AllowNull)]
        public string FullName;

        [JsonProperty("desc", Required = Required.AllowNull)]
        public string Desc
        {
            get
            {
                return mDesc;
            }
            set
            {
                mDesc = value;
                if (mDesc.Length > MaxLength)
                {
                    mDesc = mDesc.Substring(0, MaxLength - 3) + "...";
                }
            }
        }

        private string mDesc;

        [JsonProperty("url", Required = Required.AllowNull)]
        public string Url;

        [JsonProperty("adder", Required = Required.AllowNull)]
        public string Adder;

        [JsonProperty("type", Required = Required.AllowNull)]
        public SongType Type;

        [JsonProperty("tn", Required = Required.AllowNull)]
        public string ThumbNail;

        public string GetThumbnail(string Replacement)
            => ThumbNail != null && ThumbNail != string.Empty ? ThumbNail : Replacement;

        [JsonIgnore]
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
