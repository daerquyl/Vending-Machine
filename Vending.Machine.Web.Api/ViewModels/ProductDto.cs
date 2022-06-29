using Vending.Machine.Domain.Core;
using Vending.Machine.Domain.UserAccountManagement;

namespace Vending.Machine.Web.Api.ViewModels
{
    public class ProductDto
    {
        public string ProductName { get; set; }
        public decimal Cost { get; set; }
        public int AmountAvailable { get; set; }

        public ProductDto(Product product)
        {
            ProductName = product.ProductName;
            Cost = product.Cost;
            AmountAvailable = product.AmountAvailable;
        }

        public Product ToProduct(string sellerId = null) =>
            new Product(ProductName, Cost, AmountAvailable, sellerId);
    }
}
