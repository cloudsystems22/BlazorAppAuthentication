using BlazorAppAuthentication.Shared;
using Blazored.LocalStorage;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Data;

namespace BlazorAppAuthentication.Client.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _stateProvider;
        private readonly ILocalStorageService _localStorage;
        public AuthServices(HttpClient httpClient, 
            AuthenticationStateProvider authenticationStateProvider,
            ILocalStorageService localStorageService) 
        {
            _httpClient = httpClient;
            _stateProvider = authenticationStateProvider;
            _localStorage = localStorageService;
        }
        public async Task<LoginResult> Login(UserDto user)
        {
            var loginAsJson = JsonSerializer.Serialize(user);
            var content = new StringContent(loginAsJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/login", content);
            var loginResult = JsonSerializer.Deserialize<LoginResult>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!response.IsSuccessStatusCode)
                return loginResult;


            string role = string.Empty;
            if (user.Email.Equals("agneloneto@gmail.com"))
                role = "Admin";

            if (user.Email.Equals("davidfico@gmail.com"))
                role = "User";

            await _localStorage.SetItemAsync("authToken", loginResult.Token!);
            ((CustomAuthStateProvider)_stateProvider).MarkUserAsAuthenticated(user.Email!, role);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginResult.Token);
            return loginResult;
        }

        public async Task LogOut()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((CustomAuthStateProvider)_stateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
