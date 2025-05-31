using ResourceManagementSystem.Core.DTOs.User;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System; 
using System.Collections.Generic; 
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ResourceManagementSystem.UI.Web.Services
{
    public class AuthUIService
    {
        private readonly HttpClient _httpClient;
        private readonly ResourceUIService _resourceUIService;
        private readonly string _apiBaseUrl;
        private readonly LocalStorageService _localStorageService;

        private const string AuthTokenKey = "access_token"; 
        private const string UsernameKey = "username"; 

        public string? CurrentUserToken { get; private set; }
        public string? CurrentUsername { get; private set; }
        private bool _isUserLoggedIn = false;

        public event Action? AuthStateChanged;

        public AuthUIService(
            HttpClient httpClient,
            ResourceUIService resourceUIService,
            LocalStorageService localStorageService,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _resourceUIService = resourceUIService;
            _localStorageService = localStorageService;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5003";
        }

        public async Task InitializeAuthStateAsync()
        {
            CurrentUserToken = await _localStorageService.GetItemAsync<string>(AuthTokenKey);
            CurrentUsername = await _localStorageService.GetItemAsync<string>(UsernameKey);

            if (!string.IsNullOrEmpty(CurrentUserToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CurrentUserToken);
                _resourceUIService.SetAuthenticationState(true, CurrentUserToken); 
            }
            NotifyAuthStateChanged(); 
        }
        
       
        public async Task<(bool Succeeded, string[] Errors)> LoginAsync(UserLoginDto loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/Auth/Login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (authResponse?.Token != null && authResponse.Username != null)
                {
                    CurrentUserToken = authResponse.Token;
                    CurrentUsername = authResponse.Username;

                    await _localStorageService.SetItemAsync(AuthTokenKey, CurrentUserToken);
                    await _localStorageService.SetItemAsync(UsernameKey, CurrentUsername);

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CurrentUserToken);
                    _resourceUIService.SetAuthenticationState(true, CurrentUserToken);
                    NotifyAuthStateChanged();
                    return (true, Array.Empty<string>());
                }
                return (false, new[] { "Login successful, but no token or user data received." });
            }
            else
            {
                var errorResponse = await TryReadErrorResponseAsync(response);
                return (false, errorResponse?.Errors ?? new[] { $"Login failed: {response.ReasonPhrase}" });
            }
        }
        
        public async Task<(bool Succeeded, string[] Errors)> RegisterAsync(UserRegisterDto registerDto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/Auth/Register", registerDto);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (authResponse?.Token != null && authResponse.Username != null)
                {
                    CurrentUserToken = authResponse.Token;
                    CurrentUsername = authResponse.Username;

                    await _localStorageService.SetItemAsync(AuthTokenKey, CurrentUserToken);
                    await _localStorageService.SetItemAsync(UsernameKey, CurrentUsername);
                    
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CurrentUserToken);
                    _resourceUIService.SetAuthenticationState(true, CurrentUserToken);
                    NotifyAuthStateChanged();
                    return (true, Array.Empty<string>());
                }
                return (false, new[] { "Registration successful, but no token or user data received." });
            }
            else
            {
                var errorResponse = await TryReadErrorResponseAsync(response);
                return (false, errorResponse?.Errors ?? new[] { $"Registration failed: {response.ReasonPhrase}" });
            }
        }
        


        public async Task LogoutAsync()
        {

            if (!string.IsNullOrEmpty(CurrentUserToken))
            {
                try
                {
                    await _httpClient.PostAsync($"{_apiBaseUrl}/Auth/Logout", null);
                }
                catch (HttpRequestException)
                {
                    // Ignoruj błąd, jeśli API jest niedostępne, klient i tak się "wyloguje" lokalnie
                }
            }
            
            CurrentUserToken = null;
            CurrentUsername = null;
            await _localStorageService.RemoveItemAsync(AuthTokenKey);
            await _localStorageService.RemoveItemAsync(UsernameKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _resourceUIService.SetAuthenticationState(false, null);

            NotifyAuthStateChanged();
        }
        
        public bool IsUserLoggedIn() => !string.IsNullOrEmpty(CurrentUserToken);

        private void NotifyAuthStateChanged() => AuthStateChanged?.Invoke();
        
        private async Task<ErrorResponse?> TryReadErrorResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode || response.Content == null)
            {
                return null; 
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            var contentType = response.Content.Headers.ContentType?.MediaType;

            if (contentType == "application/json")
            {
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
                    if (errorResponse?.Errors != null && errorResponse.Errors.Any())
                    {
                        return errorResponse;
                    }
                    
                    var problemDetails = JsonSerializer.Deserialize<ProblemDetailsError>(responseContent);
                    if (!string.IsNullOrWhiteSpace(problemDetails?.Title))
                    {
                        var errors = new List<string> { problemDetails.Title };
                        if (problemDetails.Detail != null) errors.Add(problemDetails.Detail);
                        if (problemDetails.Errors != null) 
                        {
                            foreach (var fieldErrors in problemDetails.Errors.Values)
                            {
                                errors.AddRange(fieldErrors);
                            }
                        }
                        return new ErrorResponse { Errors = errors.ToArray() };
                    }
                }
                catch (JsonException)
                {
                    return new ErrorResponse { Errors = new[] { $"Failed to parse JSON error response. Raw content: {responseContent.Substring(0, Math.Min(responseContent.Length, 200))}" } };
                }
            }
            
            if (!string.IsNullOrWhiteSpace(response.ReasonPhrase) && responseContent.Length == 0)
            {
                 return new ErrorResponse { Errors = new[] { response.ReasonPhrase } };
            }
            
            var genericErrorMessage = $"API request failed with status {response.StatusCode}.";
            if (!string.IsNullOrWhiteSpace(responseContent))
            {
                genericErrorMessage += $" Response: {responseContent.Substring(0, Math.Min(responseContent.Length, 200))}";
            }
            else if (!string.IsNullOrWhiteSpace(response.ReasonPhrase))
            {
                 genericErrorMessage += $" Reason: {response.ReasonPhrase}";
            }
            return new ErrorResponse { Errors = new[] { genericErrorMessage } };
        }

        private class ErrorResponse
        {
            public string[]? Errors { get; set; }
        }
        
        private class ProblemDetailsError
        {
            public string? Type { get; set; }
            public string? Title { get; set; }
            public int? Status { get; set; }
            public string? Detail { get; set; }
            public string? Instance { get; set; }
            public Dictionary<string, string[]>? Errors { get; set; }
        }
    }
}