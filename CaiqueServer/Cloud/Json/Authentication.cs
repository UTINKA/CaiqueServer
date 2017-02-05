using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CaiqueServer.Cloud.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    class Authentication
    {
        internal static async Task<Authentication> UserdataFromToken(string IdToken)
        {
            var Res = await $"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={IdToken}".WebResponse();
            return JsonConvert.DeserializeObject<Authentication>(Res);
        }

        [JsonProperty("iss", Required = Required.Default)]
        public string ISS;

        [JsonProperty("aud", Required = Required.Default)]
        public string Aud;

        // Google Account ID
        [JsonProperty("sub", Required = Required.Always)]
        public string Sub;

        [JsonProperty("email_verified", Required = Required.Default)]
        public bool EmailVerified;

        [JsonProperty("azp", Required = Required.Default)]
        public string Azp;

        [JsonProperty("email", Required = Required.Default)]
        public string Email;

        [JsonProperty("iat", Required = Required.Default)]
        public int Iat;

        [JsonProperty("exp", Required = Required.Default)]
        public int Expiration;

        // Full Name
        [JsonProperty("name", Required = Required.Always)]
        public string Name;

        [JsonProperty("picture", Required = Required.Always)]
        public string Picture;

        [JsonProperty("given_name", Required = Required.Default)]
        public string FirstName;

        [JsonProperty("family_name", Required = Required.Default)]
        public string FamilyName;

        [JsonProperty("locale", Required = Required.Default)]
        public string Locale;

        [JsonProperty("alg", Required = Required.Default)]
        public string Alg;

        [JsonProperty("kid", Required = Required.Default)]
        public string KId;
    }
}
