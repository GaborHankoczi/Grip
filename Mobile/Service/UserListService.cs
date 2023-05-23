using GripMobile.Model;
using System.Net.Http.Json;

namespace GripMobile.Service
{
    public class UserListService
    {
        private List<UserDTO> users;

        public async Task<List<UserDTO>> GetUsers()
        {
            try
            {
                var response = await HttpClientSingleton.httpClient.GetAsync($"https://nloc.duckdns.org:8025/api/User");

                if (response.IsSuccessStatusCode)
                {
                    users = await response.Content.ReadFromJsonAsync<List<UserDTO>>();

                    return users;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"\tERROR {0}", exception.Message);
            }

            return users;
        }
    }
}
