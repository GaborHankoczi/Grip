using GripMobile.Model;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GripMobile.Service
{
    /// <summary>
    /// Class <c>NFCService</c> handles the attendance registration process with the server.
    /// </summary>
    public class NFCService
    {
        private readonly HttpClient httpClient;

        public NFCService() => httpClient = new HttpClient();

        public async Task<HttpStatusCode> RegisterAttendance(ActiveAttendanceDTO data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data);
                StringContent content = new(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://nloc.duckdns.org:8025/api/Attendance", content);

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
