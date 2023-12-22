using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorAppAuthentication.Client
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        public CustomAuthStateProvider(HttpClient httpClient, 
            ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            //string token = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiRGF2aWQgRmljbyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlVzZXIiLCJleHAiOjMxNjg1NDAwMDB9.F7MYwEPkyhyUH_YAb0Fw8utKWP7lAvA2NwFlvq9bXXPAD96wbwsEWQ4IDszj0yu63Zf0-iioRjDO33Ix-RODlw";
            var authToken = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(authToken))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", authToken);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimFromJwt(authToken), "jwt")));

            //var identity = new ClaimsIdentity(ParseClaimFromJwt(authToken), "jwt");
            ////var identity = new ClaimsIdentity();

            //var user = new ClaimsPrincipal(identity);
            //var state = new AuthenticationState(user);

            //NotifyAuthenticationStateChanged(Task.FromResult(state));

            //return state;
        }

        //public static IEnumerable<Claim> ParseClaimFromJwt(string jwt)
        //{
        //    var payload = jwt.Split('.')[1];
        //    var jsonBytes = ParseBase64WithoutPadding(payload);
        //    var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        //    return keyValuePairs.Select(kv => new Claim(kv.Key, kv.Value.ToString()));
        //}
        public static IEnumerable<Claim> ParseClaimFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            keyValuePairs!.TryGetValue(ClaimTypes.Role, out var roles);
            if(roles != null)
            {
                if (roles.ToString()!.Trim().StartsWith("["))
                {
                    var parseRoles = JsonSerializer.Deserialize<string[]>(roles.ToString()!);

                    foreach (var parseRole in parseRoles!)
                        claims.Add(new Claim(ClaimTypes.Role, parseRole));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!)));

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        internal void MarkUserAsAuthenticated(string email, string role)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, email), new Claim(ClaimTypes.Role, role) }, "authapi"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        internal void MarkUserAsLoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
