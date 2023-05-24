using GripMobile.Model;
using System.Net.Http.Json;

namespace GripMobile.Service
{
    public class ExemptGetService
    {
        private List<ExemptDTO> exempts;

        public async Task<List<ExemptDTO>> GetExempts()
        {
            try
            {
                var response = await HttpClientSingleton.httpClient.GetAsync($"https://nloc.duckdns.org:8025/api/Exempt");

                if (response.IsSuccessStatusCode)
                {
                    exempts = await response.Content.ReadFromJsonAsync<List<ExemptDTO>>();

                    return exempts;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"\tERROR {0}", exception.Message);
            }

            return exempts;
        }
    }
}
