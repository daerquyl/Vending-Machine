using Vending.Machine.Domain.Core;
using Vending.Machine.Domain.UserAccountManagement;

namespace Vending.Machine.Web.Api.ViewModels
{
    public class ProductDto
    {
        public string ProductName { get; set; }
        public decimal Cost { get; set; }
        public int AmountAvailable { get; private set; }
        public string SellerId { get; private set; }

        public Product ToProduct() => 
            new Product(ProductName, Cost, AmountAvailable, SellerId);    
    }
}
