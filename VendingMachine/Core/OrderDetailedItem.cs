
namespace Vending.Machine.Domain.Core
{
    public record OrderDetailedItem : OrderItem {
        public Product Product { get; }

        public OrderDetailedItem(OrderItem orderItem, Product product)
            :base(orderItem)
        {
            Product = product;
        }
    }
}
