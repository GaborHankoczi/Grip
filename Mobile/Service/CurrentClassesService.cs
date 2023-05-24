using GripMobile.Model;
using System.Net.Http.Json;

namespace GripMobile.Service
{
    public class CurrentClassesService
    {
        private List<ClassDTO> classes;

        public async Task<List<ClassDTO>> GetCurrentClasses(string date)
        {
            try
            {
                var response = await HttpClientSingleton.httpClient.GetAsync($"https://nloc.duckdns.org:8025/api/Class/OnDay/{date}");

                if (response.IsSuccessStatusCode)
                {
                    classes = await response.Content.ReadFromJsonAsync<List<ClassDTO>>();

                    return classes;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"\tERROR {0}", exception.Message);
            }

            return classes;
        }
    }
}
