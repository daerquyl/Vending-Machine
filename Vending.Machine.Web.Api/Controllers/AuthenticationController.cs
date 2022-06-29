using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Vending.Machine.Domain.UserAccountManagement;
using Vending.Machine.Domain.UserAccountManagement.Repository;
using Vending.Machine.Web.Api.ViewModels;

namespace Vending.Machine.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private const string UserNotFound = "User not found";

        private readonly IUserRepository _repository;
        private readonly IConfiguration _configuration;

        public AuthenticationController(IUserRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDto login)
        {
            var user = await _repository.GetUserByUsername(login.Username);
            if (user == null)
            {
                return BadRequest(UserNotFound);
            }

            if (!VerifyPasswordHash(login.Password,
                Convert.FromBase64String(user.Password),
                Convert.FromBase64String(user.PasswordSalt)))
            {
                return BadRequest("Wrong password");
            }

            string token = CreateToken(user);
            return Ok(token);
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
