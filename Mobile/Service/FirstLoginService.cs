using GripMobile.Model;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GripMobile.Service
{
    /// <summary>
    /// Class <c>FirstLoginService</c> handles the first login process with the server.
    /// </summary>
    public class FirstLoginService
    {
        private readonly HttpClient httpClient;
        public FirstLoginService() => httpClient = new HttpClient();

        /// <summary>
        /// Method <c>ConfirmEmail</c> sends a POST request with the given user data to the server.
        /// </summary>
        /// <param name="userData">the given user data</param>
        /// <returns>An HTTP status code representing the first login process' result.</returns>
        public async Task<HttpStatusCode> ConfirmEmail(ConfirmEmailDTO userData)
        {
            try
            {
                string json = JsonSerializer.Serialize(userData);
                StringContent content = new(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://grip.sytes.net/api/User/ConfirmEmail", content);

                return response.StatusCode;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(@"\tERROR {0}", exception.Message);
            }

            return HttpStatusCode.InternalServerError;
        }
    }
}
