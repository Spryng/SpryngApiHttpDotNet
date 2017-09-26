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
        private static readonly string ApiEndpoint_Check = "check.php";
        private static readonly string ApiEndpoint_Send = "send.php";

        private readonly string _username;
        private readonly string _password;
        private readonly string _apikey;
        private readonly bool _usePassword;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Create a new instance of the <see cref="SpryngHttpClient"/>.
        /// </summary>
        /// <param name="username">Chosen by user when signing up.</param>
        /// <param name="password">Chosen by user when signing up.</param>
        /// <param name="httpMessageHandler">The HTTP handler stack to use for sending requests.</param>
        /// <exception cref="ArgumentException">An <see cref="ArgumentException"/> if the <paramref name="username"/> or <paramref name="password"/> are invalid.</exception>
        public SpryngHttpClient(string username, string password, HttpMessageHandler httpMessageHandler = null) 
            : this(username, password, false, httpMessageHandler)
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="SpryngHttpClient"/>.
        /// </summary>
        /// <param name="username">Chosen by user when signing up.</param>
        /// <param name="secret">The apikey or Password depending on what the value of <paramref name="isApiKey"/> is.</param>
        /// <param name="isApiKey">Wether the given secret is a apikey or password.</param>
        /// <param name="httpMessageHandler">The HTTP handler stack to use for sending requests.</param>
        /// <exception cref="ArgumentException">An <see cref="ArgumentException"/> if the <paramref name="username"/> or <paramref name="secret"/> are invalid.</exception>
        public SpryngHttpClient(string username, string secret, bool isApiKey, HttpMessageHandler httpMessageHandler = null)
        {
            if (string.IsNullOrEmpty(username) || username.Length < 2 || username.Length > 32)
                throw new ArgumentException("Username must be between 2 and 32 characters.");

            _username = username;

            if (isApiKey)
            {
                _apikey = secret;
                _usePassword = false;
            }
            else
            {
                if (string.IsNullOrEmpty(secret) || secret.Length < 6 || secret.Length > 32)
                    throw new ArgumentException("Password must be between 6 and 32 characters.");

                _password = secret;
                _usePassword = true;
            }


            _httpClient = CreateHttpClient(httpMessageHandler);
        }

        /// <summary>
        /// Create a new instance of the <see cref="SpryngHttpClient"/> using a Password as authentication.
        /// </summary>
        /// <param name="username">Chosen by user when signing up.</param>
        /// <param name="password">Chosen by user when signing up.</param>
        /// <param name="httpMessageHandler">The HTTP handler stack to use for sending requests.</param>
        /// <exception cref="ArgumentException">An <see cref="ArgumentException"/> if the <paramref name="username"/> or <paramref name="password"/> are invalid.</exception>
        public static SpryngHttpClient CreateClientWithPassword(string username, string password, HttpMessageHandler httpMessageHandler = null)
        {
            return new SpryngHttpClient(
                username: username, 
                secret: password, 
                isApiKey: false,
                httpMessageHandler: httpMessageHandler);
        }

        /// <summary>
        /// Create a new instance of the <see cref="SpryngHttpClient"/> using an API Key as authentication.
        /// </summary>
        /// <param name="username">Chosen by user when signing up.</param>
        /// <param name="apikey">API Key used to authenticate.</param>
        /// <param name="httpMessageHandler">The HTTP handler stack to use for sending requests.</param>
        /// <exception cref="ArgumentException">An <see cref="ArgumentException"/> if the <paramref name="username"/> or <paramref name="password"/> are invalid.</exception>
        public static SpryngHttpClient CreateClientWithApiKey(string username, string apikey, HttpMessageHandler httpMessageHandler = null)
        {
            return new SpryngHttpClient(
                username: username,
                secret: apikey,
                isApiKey: true,
                httpMessageHandler: httpMessageHandler);
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
                var task = Task.Run(async () => await ExecuteSmsRequestAsync(request).ConfigureAwait(false));
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
            var requestData = CreateRequestDictionary();
            requestData.Add("OPERATION", "send");

            // Validate Destinations field
            if (request.Destinations == null || !request.Destinations.Any() || request.Destinations.Count() > 100)
                throw new ArgumentException("Atleast 1 destination number required and no more than 1000.");
            if (request.Destinations.Any(n => !Utilities.IsMsidnCompliant(n)))
                throw new ArgumentException("One of the Destination numbers is not MSIDN-numeric Compliant.");

            requestData.Add("DESTINATION", string.Join(",", request.Destinations));

            // Validate Sender field
            if (string.IsNullOrEmpty(request.Sender))
                throw new ArgumentException("Sender field is required.");
            if (Utilities.IsDigitsOnly(request.Sender) && request.Sender.Length > 14)
                throw new ArgumentException("Numeric senders can not be longer than 14 characters.");
            if (!Utilities.IsDigitsOnly(request.Sender) && request.Sender.Length > 11)
                throw new ArgumentException("Alphanumeric senders can not be longer than 11 characters.");

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

            requestData.Add("BODY", request.Body);

            if (!string.IsNullOrEmpty(request.Reference))
                requestData.Add("REFERENCE", request.Reference);

            if (request.EnableRawEncoding || request.EnableUnicode)
                requestData.Add("RAWENCODING", "1");
            if (request.EnableUnicode)
                requestData.Add("UNICODE", "1");

            requestData.Add("ALLOWLONG", request.AllowLong ? "1" : "0");

            // Make HTTP Request to Spryng API and Parse the result.

            var result = await ExecuteHttpRequest(ApiEndpoint_Send, requestData).ConfigureAwait(false);

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
            var requestData = CreateRequestDictionary();

            var result = await ExecuteHttpRequest(ApiEndpoint_Check, requestData).ConfigureAwait(false);

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
                var task = Task.Run(async () => await GetCreditAmountAsync().ConfigureAwait(false));
                task.Wait();
                return task.Result;
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw; // To make the compiler happy.
            }
        }

        /// <summary>
        /// Creates a new instance of the request dictionary.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> CreateRequestDictionary()
        {
            if(_usePassword)
            {
                return new Dictionary<string, string>()
                {
                    { "USERNAME", _username },
                    { "PASSWORD", _password },
                };
            }
            else
            {
                return new Dictionary<string, string>()
                {
                    { "USERNAME", _username },
                    { "SECRET", _apikey }
                };
            }
        }

        private async Task<string> ExecuteHttpRequest(string relativePath, Dictionary<string, string> parameters)
        {
            // Create the post data string using our custom URL Encoding so we can properly send special characters.
            var postData = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Utilities.CustomUrlEncode(kvp.Value)}"));

            // Use the proper encoding type depending if RAWENCODING is enabled.
            var requestEncoding = Encoding.GetEncoding("ISO-8859-1");
            if (parameters.ContainsKey("RAWENCODING"))
                requestEncoding = Encoding.UTF8;

            // Create the String Content, set it to the Encoding used by the service and make sure we send as a form.
            var stringContent = new StringContent(postData, requestEncoding, "application/x-www-form-urlencoded");

            var result = await _httpClient.PostAsync(relativePath, stringContent).ConfigureAwait(false);

            return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private HttpClient CreateHttpClient(HttpMessageHandler handler)
        {
            HttpClient httpClient;
            if (handler == null)
                httpClient = new HttpClient();
            else
                httpClient = new HttpClient(handler);

            httpClient.BaseAddress = new Uri(ApiEndpoint);
            return httpClient;
        }
    }
}
