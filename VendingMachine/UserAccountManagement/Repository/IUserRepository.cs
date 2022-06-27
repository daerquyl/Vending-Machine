using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vending.Machine.Domain.UserAccountManagement.Repository
{
    public interface IUserRepository
    {
        public IEnumerable<User> Users { get; }
        public Task<User> CreateUser(User user);
        public Task<User> RemoveUser(string userId);
        public Task UpdateUser(User user);
        public Task<User?> GetUser(string Id);
        public Task<User?> GetUserByUsername(string username);
        Task UpdateUserDeposit(string id, decimal deposit);
    }
}
