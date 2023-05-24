using GripMobile.Model;
using System.Net;
using System.Text.Json;
using System.Text;

namespace GripMobile.Service
{
    /// <summary>
    /// Class <c>ResetPasswordService</c> handles the reset password process with the server.
    /// </summary>
    public class ResetPasswordService
    {
        /// <summary>
        /// Method <c>ResetPassword</c> sends a POST request with the given user data to the server.
        /// </summary>
        /// <param name="userData">the given user data</param>
        /// <returns>An HTTP status code representing the result.</returns>
        public async Task<HttpStatusCode> ResetPassword(ResetPasswordDTO userData)
        {
            try
            {
                string json = JsonSerializer.Serialize(userData);
                StringContent content = new(json, Encoding.UTF8, "application/json");

                var response = await HttpClientSingleton.httpClient.PostAsync("https://nloc.duckdns.org:8025/api/User/ResetPassword", content);

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
