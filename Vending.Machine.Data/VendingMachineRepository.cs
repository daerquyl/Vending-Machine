using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vending.Machine.Domain.Core;
using Vending.Machine.Domain.Core.Repository;

namespace Vending.Machine.Data
{
    public class VendingMachineRepository : IVendingMachineRepository
    {
        private VendingMachineContext _context;
        private VendingMachine _vendingMachine;

        public VendingMachineRepository(VendingMachineContext context)
        {
            _context = context;
            if (_context.VendingMachines.Count() == 0)
            {
                _context.Add(new VendingMachine());
               SaveChanges();
            }
            _vendingMachine = _context.VendingMachines.First();
        }

        public async Task<Product?> CreateProduct(Product product)
        {
            _vendingMachine.LoadProduct(product);
            await SaveChanges();
            return product;
        }

        public Product? GetProduct(string id) => _vendingMachine.GetProduct(id);

        public async Task<Product?> RemoveProduct(string id)
        {
            var product = _vendingMachine.UnloadProduct(id);
            await SaveChanges();
            return product;
        }

        public async Task UpdateProduct(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await SaveChanges();
        }

        public VendingMachine GetVendingMachine() => _vendingMachine;

        public async Task CreateNewAccount(string accountId)
        {
            _vendingMachine.RegisterAccount(new VendingMachineAccount(accountId));
            await SaveChanges();
        }

        public async Task RemoveAccount(string id)
        {
            _vendingMachine.RemoveAccount(id);
            await SaveChanges();
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public bool ContainsProduct(string id)
        {
            return _vendingMachine.Products.Any(product => id == product.Id);
        }
    }
}
