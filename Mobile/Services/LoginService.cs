using GripMobile.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GripMobile.Services
{
    public class LoginService
    {
        private readonly HttpClient httpClient;

        private User user = new();

        public LoginService() => this.httpClient = new HttpClient();

        public async Task<User> CheckUserCredentials(string email, string password)
        {
            try
            {
                string json = $"{{\"email\": \"{email}\", \"password\": \"{password}\"}}";
                StringContent content = new(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://grip.sytes.net/api/User/Login", content);

                if (response.IsSuccessStatusCode)
                {
                    user = await response.Content.ReadFromJsonAsync<User>();

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
