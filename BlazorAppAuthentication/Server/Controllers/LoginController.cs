using BlazorAppAuthentication.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlazorAppAuthentication.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        //private readonly SignInManager<IdentityUser> _signInManager;
        public LoginController(IConfiguration configuration
            /*SignInManager<IdentityUser> signInManager*/)
        {
            _configuration = configuration;
            //_signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDto user)
        {
            string role = string.Empty;
            //var result = await _signInManager.PasswordSignInAsync(user.Email, user.Password, false, false);
            //if (result.Succeeded)
            //    return BadRequest(new LoginResult { Sucessful = false, Error = "Erro ao realizar login" });
            if (user.Email.Equals("agneloneto@gmail.com"))
                role = "Admin";

            if (user.Email.Equals("davidfico@gmail.com"))
                role = "User";

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email!),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JwtExpiryInDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtAudience"],
                claims,
                expires:expiry,
                signingCredentials: creds
                );

            ////string authToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new LoginResult { Sucessful = true, Token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
