using Vending.Machine.Domain.UserAccountManagement;

namespace Vending.Machine.Web.Api.ViewModels
{
    public class CreateUserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public User ToUser(string password, string passwordSalt, UserRole role)
        {
            return new User(Username, password, passwordSalt, 0, role);
        }
    }
}
