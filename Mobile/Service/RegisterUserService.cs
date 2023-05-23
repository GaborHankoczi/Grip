using GripMobile.Model;
using System.Text.Json;
using System.Text;
using System.Net.Http.Json;

namespace GripMobile.Service
{
    public class RegisterUserService
    {
        private UserDTO newUser;

        public async Task<UserDTO> RegisterUser(RegisterUserDTO data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data);
                StringContent content = new(json, Encoding.UTF8, "application/json");

                var response = await HttpClientSingleton.httpClient.PostAsync("https://nloc.duckdns.org:8025/api/User", content);

                if (response.IsSuccessStatusCode)
                { 
                    newUser = await response.Content.ReadFromJsonAsync<UserDTO>();

                    return newUser;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"\tERROR {0}", exception.Message);
            }

            return newUser;
        }
    }
}
