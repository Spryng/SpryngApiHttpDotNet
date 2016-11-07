using Spryng.Models;
using Spryng.Models.Sms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Spryng
{
    public class SpryngHttpClient
    {
        private static readonly string ApiEndpoint = "https://api.spryngsms.com/api/";
        private static readonly string ApiEndpointCheck = "check.php";
        private static readonly string ApiEndpointSend = "send.php";

        private readonly string _username;
        private readonly string _password;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Create a new instance of the <see cref="SpryngHttpClient"/>.
        /// </summary>
        /// <param name="username">Chosen by user when signing up.</param>
        /// <param name="password">Chosen by user when signing up.</param>
        /// <exception cref="ArgumentException">An <see cref="ArgumentException"/> if the <paramref name="username"/> or <paramref name="password"/> are invalid.</exception>
        public SpryngHttpClient(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || username.Length < 2 || username.Length > 32)
                throw new ArgumentException("Username must be between 2 and 32 characters.");
            if (string.IsNullOrEmpty(password) || password.Length < 6 || password.Length > 32)
                throw new ArgumentException("Password must be between 6 and 32 characters.");

            _username = username;
            _password = password;
            _httpClient = createHttpClient();
        }


        /// <summary>
        /// Execute a SMS request and get its response.
        /// </summary>
        /// <param name="request">The <see cref="SmsRequest"/> to execute.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SpryngHttpClientException"></exception>
        public void ExecuteSmsRequest(SmsRequest request)
        {
            try
            {
                var task = Task.Run(async () => { await ExecuteSmsRequestAsync(request); });
                task.Wait();
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }

        /// <summary>
        /// Execute a SMS request asynchronously and get its response.
        /// </summary>
        /// <param name="request">The <see cref="SmsRequest"/> to execute.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SpryngHttpClientException"></exception>
        public async Task ExecuteSmsRequestAsync(SmsRequest request)
        {
            // Set the predefined data for the SMS Send request.
            var requestData = new Dictionary<string, string>()
            {
                { "OPERATION", "send" },
                { "USERNAME", _username },
                { "PASSWORD", _password }
            };

            // Validate Destinations field
            if (request.Destinations == null || !request.Destinations.Any() || request.Destinations.Count() > 100)
                throw new ArgumentException("Atleast 1 destination number required and more than 1000.");
            if (request.Destinations.Any(n => !Utilities.IsMsidnCompliant(n)))
                throw new ArgumentException("One of the Destination numbers is not MSIDN-numeric Compliant.");

            requestData.Add("DESTINATION", string.Join(",", request.Destinations));

            // Validate Sender field
            if (string.IsNullOrEmpty(request.Sender))
                throw new ArgumentException("Sender field is required.");
            if (Utilities.IsDigitsOnly(request.Sender) && request.Sender.Length > 14)
                throw new ArgumentException("Numeric senders can not be longer than 14 characters long.");
            if (Utilities.IsDigitsOnly(request.Sender) && request.Sender.Length > 11)
                throw new ArgumentException("Alphanumeric senders can not be longer than 11 characters long.");

            requestData.Add("SENDER", request.Sender);

            // Validate Service
            if (!string.IsNullOrEmpty(request.Service))
                requestData.Add("SERVICE", request.Service);

            requestData.Add("ROUTE", request.Route);

            // Validate AllowLong and Body length.
            if (request.AllowLong)
            {
                if (string.IsNullOrEmpty(request.Body) || request.Body.Length > 612)
                    throw new ArgumentException("Maximum length for body is 612 characters.");
            }
            else
            {
                if (string.IsNullOrEmpty(request.Body) || request.Body.Length > 160)
                    throw new ArgumentException("Body can't be longer than 160 without enabling 'ALLOWLONG'.");
            }

            requestData.Add("ALLOWLONG", request.AllowLong ? "1" : "0");
            requestData.Add("BODY", request.Body);

            if (!string.IsNullOrEmpty(request.Reference))
                requestData.Add("REFERENCE", request.Reference);


            // Make HTTP Request to Spryng API and Parse the result.

            var result = await executeHttpRequest(ApiEndpointSend, requestData);

            var resultInt = int.Parse(result);

            // Something went wrong with the request throw a SpryngClientException.
            if (resultInt != 1)
                throw new SpryngHttpClientException(resultInt);
        }

        /// <summary>
        /// Checks the Credit amount of the currently authenticated user asynchronously.
        /// </summary>
        /// <returns>The amount of Spryng credits left.</returns>
        /// <exception cref="SpryngHttpClientException"></exception>
        public async Task<double> GetCreditAmountAsync()
        {
            var result = await executeHttpRequest(ApiEndpointCheck, new Dictionary<string, string>()
            {
                { "Username", _username },
                { "Password", _password }
            });

            double credits = double.Parse(result, CultureInfo.GetCultureInfo("en-US"));

            if (credits == -1)
                throw new SpryngHttpClientException(-1);

            return credits;
        }

        /// <summary>
        /// Checks the Credit amount of the currently authenticated user.
        /// </summary>
        /// <returns>The amount of Spryng credits left.</returns>
        /// <exception cref="SpryngHttpClientException"></exception>
        public double GetCreditAmount()
        {
            try
            {
                return GetCreditAmountAsync().Result;
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw; // To make the compiler happy.
            }
        }

        private async Task<string> executeHttpRequest(string relativePath, Dictionary<string, string> parameters)
        {
            // Create the post data string using our custom URL Encoding so we can properly send special characters.
            var postData = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Utilities.CustomUrlEncode(kvp.Value)}"));

            // Create the String Content, set it to the Encoding used by the service and make sure we send as a form.
            var stringContent = new StringContent(postData, Encoding.GetEncoding("ISO-8859-1"), "application/x-www-form-urlencoded");

            var result = await _httpClient.PostAsync(relativePath, stringContent);

            return await result.Content.ReadAsStringAsync();
        }

        private HttpClient createHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(ApiEndpoint);

            return httpClient;
        }
    }
}
