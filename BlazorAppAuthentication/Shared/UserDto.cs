using System.ComponentModel.DataAnnotations;

namespace BlazorAppAuthentication.Shared
{
    public class UserDto
    {
        [Required, DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
