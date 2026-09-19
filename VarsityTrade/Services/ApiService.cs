using Newtonsoft.Json; // Provides JSON serialization
using Newtonsoft.Json.Serialization; // Provides CamelCasePropertyNamesContractResolver
using System.Net.Http.Headers; // Provides AuthenticationHeaderValue
using System.Text; // Provides Encoding

namespace VarsityTrade.Web.Services
{
    // ApiService is the central HTTP client for all backend API calls
    // Every controller injects this service and uses it to call the API
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;

        // Newtonsoft settings — handles camelCase API responses mapped to PascalCase C# properties
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
        };

        public ApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7019";
        }

        // ─────────────────────────────────────────────────────────────
        // SET AUTH HEADER
        // ─────────────────────────────────────────────────────────────
        private void SetAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
            _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
        }

        // ─────────────────────────────────────────────────────────────
        // GET
        // ─────────────────────────────────────────────────────────────
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{endpoint}");
                if (!response.IsSuccessStatusCode) return default;
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService.GetAsync] Error: {ex.Message}");
                return default;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET WITH STATUS
        // ─────────────────────────────────────────────────────────────
        public async Task<(bool Success, T? Data, int StatusCode)> GetWithStatusAsync<T>(string endpoint)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{endpoint}");
                var statusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode) return (false, default, statusCode);
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<T>(json, _jsonSettings);
                return (true, data, statusCode);
            }
            catch
            {
                return (false, default, 0);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // POST
        // ─────────────────────────────────────────────────────────────
        public async Task<T?> PostAsync<T>(string endpoint, object body)
        {
            try
            {
                SetAuthHeader();
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/{endpoint}", content);
                if (!response.IsSuccessStatusCode) return default;
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseJson, _jsonSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService.PostAsync] Error: {ex.Message}");
                return default;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // POST WITH STATUS
        // ─────────────────────────────────────────────────────────────
        public async Task<(bool Success, T? Data, int StatusCode)> PostWithStatusAsync<T>(
            string endpoint, object body)
        {
            try
            {
                SetAuthHeader();
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/{endpoint}", content);
                var statusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode) return (false, default, statusCode);
                var responseJson = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<T>(responseJson, _jsonSettings);
                return (true, data, statusCode);
            }
            catch
            {
                return (false, default, 0);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // PUT WITH BODY
        // ─────────────────────────────────────────────────────────────
        public async Task<T?> PutAsync<T>(string endpoint, object body)
        {
            try
            {
                SetAuthHeader();
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{endpoint}", content);
                if (!response.IsSuccessStatusCode) return default;
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseJson, _jsonSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService.PutAsync] Error: {ex.Message}");
                return default;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // PUT NO BODY
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> PutAsync(string endpoint)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.PutAsync(
                    $"{_apiBaseUrl}/{endpoint}",
                    new StringContent(string.Empty, Encoding.UTF8, "application/json"));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // PUT STRING — for settings endpoint which expects plain string body
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> PutStringAsync(string endpoint, string value)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent($"\"{value}\"", Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{endpoint}", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{endpoint}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}