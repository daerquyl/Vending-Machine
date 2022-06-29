using Vending.Machine.Domain.Core;

namespace Vending.Machine.Web.Api.ViewModels
{
    public class VendingMachineDto
    {
        public decimal Deposit { get; set; }
        public List<ProductDto> Products { get; set; }

        public static VendingMachineDto ForAccount(VendingMachine vendingMachine, string accountId)
        {
            var deposit = vendingMachine.GetAccount(accountId).Deposit;
            var products = vendingMachine.Products
                .Select(product => new ProductDto(product))
                .ToList();

            return new VendingMachineDto
            {
                Deposit = deposit,
                Products = products
            };
        }
    }
}
