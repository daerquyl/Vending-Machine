using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vending.Machine.Domain.Core.Repository
{
    public interface IVendingMachineRepository
    {
        VendingMachine GetVendingMachine();
        Task<Product?> CreateProduct(Product product);
        Product? GetProduct(string id);
        Task UpdateProduct(Product product);
        Task<Product?> RemoveProduct(string id);
        bool ContainsProduct(string id);
        Task SaveChanges();
        Task CreateNewAccount(string id);
        Task RemoveAccount(string id);
    }
}
