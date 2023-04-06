using GripMobile.Model;
using System.Text;
using System.Text.Json;

namespace GripMobile.Service
{
    public class FirstLoginService
    {
        private readonly HttpClient httpClient;
        public FirstLoginService() => httpClient = new HttpClient();

        public async Task<bool> ConfirmEmail(ConfirmEmailDTO userData)
        {
            try
            {
                string json = JsonSerializer.Serialize(userData);
                StringContent content = new(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://grip.sytes.net/api/User/ConfirmEmail", content);

                if(response.IsSuccessStatusCode) return true;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(@"\tERROR {0}", exception.Message);
            }

            return false;
        }
    }
}
