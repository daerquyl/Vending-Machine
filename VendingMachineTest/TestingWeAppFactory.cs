using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vending.Machine.Data;
using Vending.Machine.Domain.Core;
using Vending.Machine.Domain.UserAccountManagement;

namespace VendingMachineTest
{
    public class TestingWeAppFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<VendingMachineContext>));
                if (descriptor != null)
                    services.Remove(descriptor);
                services.AddDbContext<VendingMachineContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryEmployeeTest");
                });
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                using (var appContext = scope.ServiceProvider.GetRequiredService<VendingMachineContext>())
                {
                    try
                    {
                        appContext.Database.EnsureCreated();
                        // Seed the database with test data.
                        SeedVendingMachineBeforeTests(appContext);
                    }
                    catch (Exception ex)
                    {
                        //Log errors or do anything you think it's needed
                        throw;
                    }
                }
            });
        }

        private void SeedVendingMachineBeforeTests(VendingMachineContext appContext)
        {
            var bPassword = "Fu1cc40 + 65tczz8uCEE3CYsH8K4rGh8uVh8evnYk6EjSUyD0Ja0Ler42bAVQ4u9bY3x6ZBLbw2MLSwHUZhTCpg ==";
            var bPasswordSalt = "ME5wTHd8U8lYr+5JmHpscab2BdIIu1IZWMavfFPevcCUESVnsNL5mmM4AxQ6qUfuhVSQlLwoDsjKeYRApCfMLv26idFLQgs/g4g3+yEQagEeXZcMK2FF8JvUooDF0AckalEiVTa60X6M9ZDnsDxBpBJ6HQySbYFGa4B7EQtljvQ=";
            var buyer = new User("buyer", bPassword, bPasswordSalt, 0, UserRole.Buyer);
            
            var seller = new User("seller", "password", "psalt", 0, UserRole.Seller);

            appContext.Users.Add(buyer);
            appContext.Users.Add(seller);

            var product = new Product("product", 0.5m, 10, buyer.Id);
            product.Id = "productId";
            var account = new VendingMachineAccount(seller.Id);
            account.Id = buyer.Id;


            var vendingMachine = new VendingMachine();
            vendingMachine.LoadProduct(product);
            vendingMachine.RegisterAccount(account);
            vendingMachine.MakeDeposit(account.Id, Money.FiftyCentEuros);

            appContext.Add(vendingMachine);
            appContext.SaveChanges();
        }
    }
}
