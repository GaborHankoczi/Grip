using GripMobile.Model;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace GripMobile.Service
{
    /// <summary>
    /// Class <c>LoginService</c> handles the standard login process with the server.
    /// </summary>
    public class LoginService
    {
        private readonly HttpClient httpClient;

        /// <value>Property <c>user</c> stores the server's response.</value>
        private LoginResultDTO user = new();

        public LoginService() => httpClient = new HttpClient();

        /// <summary>
        /// Method <c>CheckUserCredentials</c> sends a POST request with the given user data to the server.
        /// </summary>
        /// <param name="userData"> represents the data the user has given in the login Activity.</param>
        /// <returns>An empty or a filled <c>LoginResultDTO</c> according to the connection's success.</returns>
        public async Task<LoginResultDTO> CheckUserCredentials(LoginUserDTO userData)
        {
            try
            {
                string json = JsonSerializer.Serialize(userData);
                StringContent content = new(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://grip.sytes.net/api/User/Login", content);

                if (response.IsSuccessStatusCode)
                {
                    user = await response.Content.ReadFromJsonAsync<LoginResultDTO>();

                    return user;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(@"\tERROR {0}", exception.Message);
            }

            return user;
        }
    }
}
