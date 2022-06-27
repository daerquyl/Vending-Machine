using Vending.Machine.Domain.Common;

namespace Vending.Machine.Domain.Core
{
    public class Product : Entity
    {
        public string ProductName { get; set; }
        public decimal Cost { get; set;  }
        public int AmountAvailable { get; private set; }
        public string SellerId { get; private set; }

        public Product() { }
        public Product(string productName, decimal cost, int amountAvailable, string sellerId)
        {
            ProductName = productName;
            Cost = cost;
            AmountAvailable = amountAvailable;
            SellerId = sellerId;
        }

        public void Substract(int amountOfProducts)
        {
            if(amountOfProducts < 0)
            {
                throw new InvalidOperationException("Not a valid amount to substract");
            }

            if (CanSubstract(amountOfProducts))
            {
                AmountAvailable -= amountOfProducts;
            }
        }

        public bool CanSubstract(int amountToSubstract)
        {
            return amountToSubstract <= AmountAvailable;
        }

        public void Add(int amountToAdd)
        {
            this.AmountAvailable += amountToAdd;
        }
    }
}
