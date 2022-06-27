using Microsoft.EntityFrameworkCore;
using Vending.Machine.Domain.UserAccountManagement;
using Vending.Machine.Domain.UserAccountManagement.Repository;

namespace Vending.Machine.Data
{
    public class UserRepository: IUserRepository
    {
        private readonly VendingMachineContext _context;

        public UserRepository(VendingMachineContext context)
        {
            _context = context;
        }

        public IEnumerable<User> Users => _context.Users;

        public async Task<User> CreateUser(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUser(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _context.Users.SingleOrDefaultAsync(user => user.Username == username);
        }

        public async Task<User?> RemoveUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return user;
        }

        public async Task UpdateUser(User user)
        {
            var UserToUpdate = await _context.Users.FindAsync(user.Id);
            if (UserToUpdate != null)
            {
                UserToUpdate.Username = user.Username ?? UserToUpdate.Username;
                UserToUpdate.Role = user.Role;
                await _context.SaveChangesAsync();
            }
         }

        public async Task UpdateUserDeposit(string id, decimal deposit)
        {
            var user = await _context.Users.FindAsync(id);
            if(user != null)
            {
                user.Deposit = deposit;
            }
            await _context.SaveChangesAsync();
        }
    }
}
