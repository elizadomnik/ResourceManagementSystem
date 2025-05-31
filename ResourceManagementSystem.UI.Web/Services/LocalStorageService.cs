using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Tasks;

namespace ResourceManagementSystem.UI.Web.Services
{
    public class LocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            var jsonValue = JsonSerializer.Serialize(value);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, jsonValue);
        }

        public async Task<T?> GetItemAsync<T>(string key)
        {
            var jsonValue = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            if (string.IsNullOrEmpty(jsonValue))
            {
                return default;
            }
            try
            {
                return JsonSerializer.Deserialize<T>(jsonValue);
            }
            catch
            {
                return default;
            }
        }

        public async Task RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
}