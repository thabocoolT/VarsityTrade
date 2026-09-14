using Newtonsoft.Json; // Provides JSON serialization and deserialization
using System.Net.Http.Headers; // Provides AuthenticationHeaderValue for Bearer token
using System.Text; // Provides Encoding for request body serialization

namespace VarsityTrade.Web.Services
{
    // ApiService is the central HTTP client for all backend API calls
    // Every controller injects this service and uses it to call the API
    // It handles authentication, serialization, and error handling in one place
    public class ApiService
    {
        // HttpClient is injected via dependency injection — manages connection pooling
        private readonly HttpClient _httpClient;

        // IHttpContextAccessor lets us read the JWT token from the current user's session
        private readonly IHttpContextAccessor _httpContextAccessor;

        // The base URL of the backend API — read from appsettings.json
        private readonly string _apiBaseUrl;

        // Constructor receives dependencies via dependency injection
        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;

            // Read the API base URL from appsettings.json
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"]
                ?? "https://localhost:7019";
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPER — SET AUTH HEADER
        // Reads the JWT token from the session and attaches it to the request
        // Called before every API request that requires authentication
        // ─────────────────────────────────────────────────────────────
        private void SetAuthHeader()
        {
            // Read the token stored in the session after login
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");

            if (!string.IsNullOrEmpty(token))
            {
                // Attach the Bearer token to the Authorization header
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // Clear the auth header if no token is present
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET — sends a GET request and deserializes the response
        // ─────────────────────────────────────────────────────────────
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            SetAuthHeader();

            // Send the GET request to the API
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{endpoint}");

            if (!response.IsSuccessStatusCode)
                return default; // Return null/default if the request failed

            // Read and deserialize the response body
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        // ─────────────────────────────────────────────────────────────
        // POST — sends a POST request with a JSON body
        // ─────────────────────────────────────────────────────────────
        public async Task<T?> PostAsync<T>(string endpoint, object body)
        {
            SetAuthHeader();

            // Serialize the request body to JSON
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send the POST request
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/{endpoint}", content);

            if (!response.IsSuccessStatusCode)
                return default;

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseJson);
        }

        // ─────────────────────────────────────────────────────────────
        // PUT — sends a PUT request with a JSON body
        // ─────────────────────────────────────────────────────────────
        public async Task<T?> PutAsync<T>(string endpoint, object body)
        {
            SetAuthHeader();

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{endpoint}", content);

            if (!response.IsSuccessStatusCode)
                return default;

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseJson);
        }

        // ─────────────────────────────────────────────────────────────
        // PUT NO BODY — sends a PUT request with no body
        // Used for toggle and action endpoints like /accept, /reject
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> PutAsync(string endpoint)
        {
            SetAuthHeader();

            var response = await _httpClient.PutAsync(
                $"{_apiBaseUrl}/{endpoint}",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE — sends a DELETE request
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(string endpoint)
        {
            SetAuthHeader();

            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{endpoint}");
            return response.IsSuccessStatusCode;
        }

        // ─────────────────────────────────────────────────────────────
        // POST WITH STATUS — returns the HTTP status code
        // Used for auth endpoints where we need to check the status code
        // ─────────────────────────────────────────────────────────────
        public async Task<(bool Success, T? Data, int StatusCode)> PostWithStatusAsync<T>(
            string endpoint, object body)
        {
            SetAuthHeader();

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/{endpoint}", content);
            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
                return (false, default, statusCode);

            var responseJson = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<T>(responseJson);
            return (true, data, statusCode);
        }

        // ─────────────────────────────────────────────────────────────
        // GET WITH STATUS — returns the HTTP status code alongside the data
        // Used when we need to distinguish between 404 (not found) and
        // 401 (unauthorized) vs 200 (success)
        // ─────────────────────────────────────────────────────────────
        public async Task<(bool Success, T? Data, int StatusCode)> GetWithStatusAsync<T>(string endpoint)
        {
            SetAuthHeader();

            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{endpoint}");
            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
                return (false, default, statusCode);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<T>(json);
            return (true, data, statusCode);
        }
    }
}