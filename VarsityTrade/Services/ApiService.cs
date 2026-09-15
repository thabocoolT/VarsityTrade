using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace VarsityTrade.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;

        public ApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;

            _apiBaseUrl = (
                configuration["ApiSettings:BaseUrl"]
                ?? "https://localhost:7019"
            ).TrimEnd('/');
        }

        // Creates a request with the current user's JWT.
        private HttpRequestMessage CreateRequest(
            HttpMethod method,
            string endpoint)
        {
            var request = new HttpRequestMessage(
                method,
                $"{_apiBaseUrl}/{endpoint.TrimStart('/')}");

            var token = _httpContextAccessor
                .HttpContext?
                .Session
                .GetString("AccessToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return request;
        }

        // ─────────────────────────────────────────────────────────────
        // GET
        // ─────────────────────────────────────────────────────────────

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var result = await GetWithStatusAsync<T>(endpoint);

            return result.Success
                ? result.Data
                : default;
        }

        // ─────────────────────────────────────────────────────────────
        // GET WITH STATUS
        // ─────────────────────────────────────────────────────────────

        public async Task<(bool Success, T? Data, int StatusCode)>
            GetWithStatusAsync<T>(string endpoint)
        {
            using var request =
                CreateRequest(HttpMethod.Get, endpoint);

            using var response =
                await _httpClient.SendAsync(request);

            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                return (false, default, statusCode);
            }

            var json =
                await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
            {
                return (true, default, statusCode);
            }

            try
            {
                var data =
                    JsonConvert.DeserializeObject<T>(json);

                return (true, data, statusCode);
            }
            catch (JsonException)
            {
                return (false, default, statusCode);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // POST
        // ─────────────────────────────────────────────────────────────

        public async Task<T?> PostAsync<T>(
            string endpoint,
            object body)
        {
            var result =
                await PostWithStatusAsync<T>(endpoint, body);

            return result.Success
                ? result.Data
                : default;
        }

        // ─────────────────────────────────────────────────────────────
        // POST WITH STATUS
        // ─────────────────────────────────────────────────────────────

        public async Task<(bool Success, T? Data, int StatusCode)>
            PostWithStatusAsync<T>(
                string endpoint,
                object body)
        {
            using var request =
                CreateRequest(HttpMethod.Post, endpoint);

            var json =
                JsonConvert.SerializeObject(body);

            request.Content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

            using var response =
                await _httpClient.SendAsync(request);

            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                return (false, default, statusCode);
            }

            var responseJson =
                await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                return (true, default, statusCode);
            }

            try
            {
                var data =
                    JsonConvert.DeserializeObject<T>(
                        responseJson);

                return (true, data, statusCode);
            }
            catch (JsonException)
            {
                return (false, default, statusCode);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // PUT WITH BODY
        // ─────────────────────────────────────────────────────────────

        public async Task<T?> PutAsync<T>(
            string endpoint,
            object body)
        {
            using var request =
                CreateRequest(HttpMethod.Put, endpoint);

            var json =
                JsonConvert.SerializeObject(body);

            request.Content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

            using var response =
                await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var responseJson =
                await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                return default;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(
                    responseJson);
            }
            catch (JsonException)
            {
                return default;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // PUT WITHOUT BODY
        // ─────────────────────────────────────────────────────────────

        public async Task<bool> PutAsync(string endpoint)
        {
            using var request =
                CreateRequest(HttpMethod.Put, endpoint);

            request.Content =
                new StringContent(
                    string.Empty,
                    Encoding.UTF8,
                    "application/json");

            using var response =
                await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────────────────────────

        public async Task<bool> DeleteAsync(string endpoint)
        {
            using var request =
                CreateRequest(HttpMethod.Delete, endpoint);

            using var response =
                await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}