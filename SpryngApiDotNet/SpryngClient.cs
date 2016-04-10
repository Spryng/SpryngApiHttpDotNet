using Spryng.Models.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Spryng
{
    public class SpryngClient
    {
        private static readonly string ApiEndpoint = "https://www.spryng.nl";
        private static readonly string ApiEndpointCheck = "/check.php";
        private static readonly string ApiEndpointSend = "/send.php";

        private readonly string _username;
        private readonly string _password;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Create a new instance of the <see cref="SpryngClient"/>.
        /// </summary>
        /// <param name="username">Chosen by user when signing up.</param>
        /// <param name="password">Chosen by user when signing up.</param>
        public SpryngClient(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Spryng Username cannot be empty");
            else if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Spryng Password cannot be empty");

            _username = username;
            _password = password;
            _httpClient = createHttpClient();
        }

        /// <summary>
        /// Execute a SMS request and get its response.
        /// </summary>
        /// <param name="request">The <see cref="SmsRequest"/> to execute.</param>
        /// <returns></returns>
        public int ExecuteSmsRequest(SmsRequest request)
        {
            return 0;
        }

        /// <summary>
        /// Checks the Credit amount of the currently authenticated user.
        /// </summary>
        /// <returns>The amount of Spryng credits left.</returns>
        public async Task<double> GetCreditAmountAsync()
        {
            var result = await executeHttpRequest(ApiEndpointCheck, new Dictionary<string, string>()
            {
                { "Username", _username },
                { "Password", _password }
            });

            return double.Parse(result);
        }

        /// <summary>
        /// Checks the Credit amount of the currently authenticated user.
        /// </summary>
        /// <returns>The amount of Spryng credits left.</returns>
        public double GetCreditAmount()
        {
            return GetCreditAmountAsync().Result;
        }


        private async Task<string> executeHttpRequest(string relativePath, Dictionary<string, string> parameters)
        {
            var result = await _httpClient.PostAsync(relativePath, new FormUrlEncodedContent(parameters));

            return await result.Content.ReadAsStringAsync();
        }

        private HttpClient createHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(ApiEndpoint);
            httpClient.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");

            return httpClient;
        }
    }
}
