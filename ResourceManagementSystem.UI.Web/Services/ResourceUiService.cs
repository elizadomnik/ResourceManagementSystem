using Microsoft.AspNetCore.SignalR.Client;
using ResourceManagementSystem.Core.DTOs.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ResourceManagementSystem.UI.Web.Services
{
    public class ResourceUIService : IAsyncDisposable
    {
        private readonly HttpClient _httpClient;
        private HubConnection? _hubConnection;
        private List<ResourceDto> _resources = new();
        private readonly string _apiBaseUrl;
        private readonly string _hubUrl;

        public event Action? ResourcesChanged;

        private bool _isAuthenticated = false;
        private string? _currentTokenForSignalR;

        public ResourceUIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5003";
            _hubUrl = $"{_apiBaseUrl}/Hubs/Resource";
        }

        public async Task SetAuthenticationState(bool isAuthenticated, string? token)
        {
            bool stateJustChanged = _isAuthenticated != isAuthenticated || _currentTokenForSignalR != token;
            _isAuthenticated = isAuthenticated;
            _currentTokenForSignalR = token;
            
            if (stateJustChanged)
            {
                if (_isAuthenticated)
                {
                    await InitializeAsync();
                }
                else
                {
                    if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
                    {
                        await _hubConnection.StopAsync();
                        Console.WriteLine("SignalR Disconnected (due to logout).");
                    }
                    if (_resources.Any())
                    {
                        _resources.Clear();
                        NotifyStateChanged();
                    }
                }
            }
        }

        public async Task InitializeAsync()
        {
            if (!_isAuthenticated)
            {
                return;
            }

            if (_hubConnection == null || _hubConnection.State == HubConnectionState.Disconnected)
            {
                _hubConnection = new HubConnectionBuilder()
                   .WithUrl(_hubUrl, options =>
                    {
                        if (!string.IsNullOrEmpty(_currentTokenForSignalR))
                        {
                            options.AccessTokenProvider = () => Task.FromResult(_currentTokenForSignalR);
                        }
                    })
                   .WithAutomaticReconnect()
                   .Build();

                _hubConnection.On<ResourceDto>("ReceiveResourceCreated", (resource) =>
                {
                    var existing = _resources.FirstOrDefault(r => r.Id == resource.Id);
                    if (existing == null) _resources.Add(resource);
                    else _resources[_resources.IndexOf(existing)] = resource;
                    _resources = _resources.OrderBy(r => r.Name).ToList();
                    NotifyStateChanged();
                });

                _hubConnection.On<ResourceDto>("ReceiveResourceUpdate", (resource) =>
                {
                    var index = _resources.FindIndex(r => r.Id == resource.Id);
                    if (index != -1)
                    {
                        _resources[index] = resource;
                    }
                    else
                    {
                        _resources.Add(resource);
                    }
                    _resources = _resources.OrderBy(r => r.Name).ToList();
                    NotifyStateChanged();
                });

                _hubConnection.On<Guid>("ReceiveResourceDeleted", (resourceId) =>
                {
                    var resource = _resources.FirstOrDefault(r => r.Id == resourceId);
                    if (resource != null)
                    {
                        _resources.Remove(resource);
                        NotifyStateChanged();
                    }
                });

                try
                {
                    await _hubConnection.StartAsync();
                    Console.WriteLine($"SignalR Connected to {_hubUrl}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting to SignalR Hub: {ex.Message}{(ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")}");
                    return;
                }
            }
            await LoadInitialResourcesAsync();
        }

        public IReadOnlyList<ResourceDto> GetResources() => _resources.AsReadOnly();

        private async Task LoadInitialResourcesAsync()
        {
            if (!_isAuthenticated)
            {
                if (_resources.Any()) // Wyczyść tylko jeśli coś jest
                {
                    _resources.Clear();
                    NotifyStateChanged();
                }
                return;
            }

            try
            {
                var loadedResources = await _httpClient.GetFromJsonAsync<List<ResourceDto>>($"{_apiBaseUrl}/Resources");
                _resources = loadedResources ?? new List<ResourceDto>();
                _resources = _resources.OrderBy(r => r.Name).ToList();
                NotifyStateChanged();
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Unauthorized access when loading resources.");
                if (_resources.Any())
                {
                    _resources.Clear();
                    NotifyStateChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading initial resources: {ex.Message}");
            }
        }

        public async Task CreateResourceAsync(CreateResourceDto newResource)
        {
            if (!_isAuthenticated) throw new InvalidOperationException("User is not authenticated to create resources.");
            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/Resources", newResource);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateResourceAsync(Guid id, UpdateResourceDto updatedResource)
        {
            if (!_isAuthenticated) throw new InvalidOperationException("User is not authenticated to update resources.");
            var response = await _httpClient.PutAsJsonAsync($"{_apiBaseUrl}/Resources/{id}", updatedResource);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteResourceAsync(Guid id)
        {
            if (!_isAuthenticated) throw new InvalidOperationException("User is not authenticated to delete resources.");
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/Resources/{id}");
            response.EnsureSuccessStatusCode();
        }
        
        private void NotifyStateChanged() => ResourcesChanged?.Invoke();

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}