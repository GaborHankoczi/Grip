using GripMobile.Model;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace GripMobile.Service
{
    public class GiveExemptService
    {
        private ExemptDTO createdExempt;

        public async Task<ExemptDTO> GiveExempt(CreateExemptDTO data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data);
                StringContent content = new(json, Encoding.UTF8, "application/json");

                var response = await HttpClientSingleton.httpClient.PostAsync("https://nloc.duckdns.org:8025/api/Exempt", content);

                if (response.IsSuccessStatusCode)
                {
                    createdExempt = await response.Content.ReadFromJsonAsync<ExemptDTO>();

                    return createdExempt;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"\tERROR {0}", exception.Message);
            }

            return createdExempt;
        }
    }
}
