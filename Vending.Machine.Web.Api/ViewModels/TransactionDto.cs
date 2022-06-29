using Vending.Machine.Domain.Core;

namespace Vending.Machine.Web.Api.ViewModels
{
    public class TransactionDto
    {
        public List<OrderDetailedItem> PurchasedItems { get; set; }
        public decimal TotalSpent { get; set; }
        public MoneyDto Change { get; set; }
        public static TransactionDto FromTransaction(Transaction transaction)
        {
            return new TransactionDto
            {
                PurchasedItems = transaction.PurchasedItems.ToList(),
                TotalSpent = transaction.Total,
                Change = MoneyDto.FromMoney(transaction.Change)
            };
        }
    }
}
