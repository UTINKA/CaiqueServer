using CaiqueServer.Firebase.Json;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CaiqueServer.Firebase
{
    class Authentication
    {
        internal static async Task<AuthenticationResponse> GetUnique(string IdToken)
        {
            var Res = await $"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={IdToken}".WebResponse();
            return JsonConvert.DeserializeObject<AuthenticationResponse>(Res);
        }
    }
}
