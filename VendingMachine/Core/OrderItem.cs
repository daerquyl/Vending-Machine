namespace Vending.Machine.Domain.Core
{
    public record OrderItem(string ProductId, int AmountOfProducts, decimal Cost);
}
