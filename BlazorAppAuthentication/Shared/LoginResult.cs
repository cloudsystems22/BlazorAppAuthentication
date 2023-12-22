namespace BlazorAppAuthentication.Shared
{
    public class LoginResult
    {
        public bool Sucessful { get; set; }

        public string? Error { get; set; }

        public string? Token { get; set; }
    }
}
