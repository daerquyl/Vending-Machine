using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Vending.Machine.Domain.UserAccountManagement;
using Vending.Machine.Domain.UserAccountManagement.Repository;
using Vending.Machine.Web.Api.ViewModels;
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

        public UserController(IUserRepository repository, IVendingMachineRepository vendingMachineRepository, IConfiguration configuration)
        {
            _repository = repository;
            _vendingMachineRepository = vendingMachineRepository;
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

        private bool UserExists(string search)
        {
            return _repository.Users.Any(e => e.Id == search || e.Username == search);
        }

        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = Convert.ToBase64String(hmac.Key);
                passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }

    }
}
