namespace Vending.Machine.Domain.Core
{
    public enum TransactionStatus
    {
        Success, Canceled
    }
    public class Transaction
    {
        private List<OrderDetailedItem> _purchasedItems;

        public VendingMachineAccount Account { get; }
        private IReadOnlyCollection<OrderItem> BuyOrder { get;  }
        public IReadOnlyCollection<OrderDetailedItem> PurchasedItems => _purchasedItems;
        public Money? Change { get;  private set; }
        public TransactionStatus Status { get; private set; }

        public decimal Total => PurchasedItems.Sum(item => item.AmountOfProducts * item.Cost);

        public Transaction() { }
        public Transaction(VendingMachineAccount account, IReadOnlyCollection<OrderItem> buyOrder)
        {
            Account = account;
            BuyOrder = buyOrder;
            _purchasedItems = new List<OrderDetailedItem>();
        }

        public void RecordPurchasedItem(OrderDetailedItem item) {
            if(item != null)
            {
                _purchasedItems.Add(item);
            }
        }

        public void RecordChange(Money change) => this.Change = change;

        public void Commit() => Status = TransactionStatus.Success;
        public void Rollback() => Status = TransactionStatus.Canceled;
    }
}