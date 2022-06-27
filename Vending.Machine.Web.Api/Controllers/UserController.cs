using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vending.Machine.Domain.UserAccountManagement;
using Vending.Machine.Domain.UserAccountManagement.Repository;
using Vending.Machine.Web.Api.ViewModels;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Vending.Machine.Domain.Core.Repository;

namespace Vending.Machine.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private const string UserNotFound = "User not found";
        private const string UserAlreadyExists = "User already exists";
        private const string InvalidRole = "Role is not valid";

        private IUserRepository _repository;
        private readonly IVendingMachineRepository _vendingMachineRepository;
        private readonly IConfiguration _configuration;

        public UserController(IUserRepository repository, IVendingMachineRepository vendingMachineRepository, IConfiguration configuration)
        {
            _repository = repository;
            _vendingMachineRepository = vendingMachineRepository;
            _configuration = configuration;
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            var user = await _repository.GetUser(id);

            if (user == null)
            {
                return NotFound(UserNotFound);
            }

            return UserDto.From(user);
        }

        // POST: api/User
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto request)
        {
            if (UserExists(request.Username))
            {
                return BadRequest(UserAlreadyExists);
            }
            if(!Enum.TryParse<UserRole>(request.Role, out var role))
            {
                return BadRequest(InvalidRole);
            };
            CreatePasswordHash(request.Password, out string passwordHash, out string passwordSalt);

            var user = request.ToUser(passwordHash, passwordSalt, role);

            await _repository.CreateUser(user);
            await _vendingMachineRepository.CreateNewAccount(user.Id);//Should Handle this with domain Event
            return CreatedAtAction("GetUser", new { id = user.Id }, UserDto.From(user));
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, User request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            try
            {
                await _repository.UpdateUser(request);
            }
            catch when(!UserExists(id))
            {
                return NotFound(UserNotFound);
            }

            return NoContent();
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _repository.RemoveUser(id);
            if (user == null)
            {
                return NotFound(UserNotFound);
            }

            //Todo: Handle using Domain Event
            await _vendingMachineRepository.RemoveAccount(id);
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDto login)
        {
            var user = await _repository.GetUserByUsername(login.Username);
            if (user == null)
            {
                return BadRequest(UserNotFound);
            }

            if(!VerifyPasswordHash(
                login.Password,
                Encoding.UTF8.GetBytes(user.Password),
                Encoding.UTF8.GetBytes(user.PasswordSalt)))
            {
                return BadRequest("Wrong password");
            }

            string token = CreateToken(user);
            return Ok(token);
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

        private bool UserExists(string search)
        {
            return _repository.Users.Any(e => e.Id == search || e.Username == search);
        }

        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = Encoding.UTF8.GetString(hmac.Key);
                passwordHash = Encoding.UTF8.GetString(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
