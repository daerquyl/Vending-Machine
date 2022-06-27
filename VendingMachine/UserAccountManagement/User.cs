using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vending.Machine.Domain.Common;

namespace Vending.Machine.Domain.UserAccountManagement
{
    public enum UserRole
    {
        Seller, Buyer
    }

    public class User: Entity
    {
        public User() { }
        public User(string username, string password, string passwordSalt, decimal deposit, UserRole role)
        {
            Username = username;
            Password = password;
            PasswordSalt = passwordSalt;
            Deposit = deposit;
            Role = role;
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public decimal Deposit { get; set; }
        public UserRole Role { get; set; }
    }
}
