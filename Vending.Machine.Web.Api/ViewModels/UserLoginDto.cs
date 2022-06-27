using Vending.Machine.Domain.Core;
using Vending.Machine.Domain.UserAccountManagement;

namespace Vending.Machine.Web.Api.ViewModels
{
    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }  
    }
}
