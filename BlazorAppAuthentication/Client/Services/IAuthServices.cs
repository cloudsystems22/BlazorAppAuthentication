using BlazorAppAuthentication.Shared;

namespace BlazorAppAuthentication.Client.Services
{
    public interface IAuthServices
    {
        Task<LoginResult> Login(UserDto user);

        Task LogOut();
    }
}
