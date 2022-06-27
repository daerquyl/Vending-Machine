using Vending.Machine.Domain.UserAccountManagement;

namespace Vending.Machine.Web.Api.ViewModels
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public decimal Deposit { get; set; }

        public static UserDto From(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role.ToString(),
                Deposit = user.Deposit
            };
        }
    }
}
