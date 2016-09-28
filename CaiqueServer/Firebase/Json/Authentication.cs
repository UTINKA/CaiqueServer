using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CaiqueServer.Firebase.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class Authentication
    {
        internal static async Task<Authentication> GetUnique(string IdToken)
        {
            var Res = await $"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={IdToken}".WebResponse();
            return JsonConvert.DeserializeObject<Authentication>(Res);
        }

        [JsonProperty("iss", Required = Required.Always)]
        public string ISS;

        [JsonProperty("aud", Required = Required.Always)]
        public string Aud;

        [JsonProperty("sub", Required = Required.Always)]
        public string Sub;

        [JsonProperty("email_verified", Required = Required.Always)]
        public bool EmailVerified;

        [JsonProperty("azp", Required = Required.Always)]
        public string Azp;

        [JsonProperty("email", Required = Required.Always)]
        public string Email;

        [JsonProperty("iat", Required = Required.Always)]
        public int Iat;

        [JsonProperty("exp", Required = Required.Always)]
        public int Expiration;

        [JsonProperty("name", Required = Required.Always)]
        public string Name;

        [JsonProperty("picture", Required = Required.Always)]
        public string Picture;

        [JsonProperty("given_name", Required = Required.Always)]
        public string FirstName;

        [JsonProperty("family_name", Required = Required.Always)]
        public string FamilyName;

        [JsonProperty("locale", Required = Required.Always)]
        public string Locale;

        [JsonProperty("alg", Required = Required.Always)]
        public string Alg;

        [JsonProperty("kid", Required = Required.Always)]
        public string KId;
    }
}
