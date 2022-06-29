using Vending.Machine.Domain.Common;

namespace Vending.Machine.Domain.Core
{
    public class VendingMachine : Entity
    {
        private List<Product> _products;
        private List<VendingMachineAccount> _accounts;

        public IReadOnlyCollection<Product> Products => _products;
        public IReadOnlyCollection<VendingMachineAccount> Accounts => _accounts;

        public Money Money { get; private set; } = Money.None;

        public VendingMachine()
        {
            _products = new List<Product>();
            _accounts = new List<VendingMachineAccount>();
        }

        public void LoadProduct(Product product)
        {
            if(product == null)
            {
                throw new ArgumentNullException();
            }
            if(product.AmountAvailable <= 0)
            {
                throw new InvalidOperationException("No product to load");
            }
            if(product.Cost <= 0)
            {
                throw new InvalidOperationException("Product must have a positive cost");
            }

            _products.Add(product);
        }

        public void RegisterAccount(VendingMachineAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException();
            }
            _accounts.Add(account);
        }

        public Transaction BuyProduct(string accountId, string productId, int amountOfProducts)
        {
            var product = GetProduct(productId);
            if(product == null)
            {
                throw new InvalidOperationException($"No Product with Id : {productId}");
            }
            var order = new List<OrderItem>() { new OrderItem(productId, amountOfProducts, product.Cost) };
            return BuyProducts(accountId, order);
        }

        public Transaction BuyProducts(string accountId, IReadOnlyCollection<OrderItem> buyOrder)
        {
            var account = GetAccount(accountId);
            if (account == null)
            {
                throw new InvalidOperationException($"No Account with Id : {accountId}");
            }

            var transaction = new Transaction(account, buyOrder);

            buyOrder.ToList().ForEach(item => transaction.RecordPurchasedItem(BuyProduct(item, account)));

            ReturnChangeIfAny(transaction);

            return transaction;
        }

        public void RemoveAccount(string id)
        {
            var account = GetAccount(id);
            if(account != null)
            {
                _accounts.Remove(account);
            }
        }

        private OrderDetailedItem? BuyProduct(OrderItem item, VendingMachineAccount account)
        {
            var product = GetProduct(item.ProductId);
            if (product == null)
            {
                return null;
            }

            var debitAmount = item.AmountOfProducts * item.Cost;
            if (!account.CanDebit(debitAmount) || !product.CanSubstract(item.AmountOfProducts))
            {
                return null;
            }

            account.Debit(debitAmount);
            product.Substract(item.AmountOfProducts);

            return new OrderDetailedItem(item, product);
        }

        private void ReturnChangeIfAny(Transaction transaction)
        {
            var balanceAfterPurchased = transaction.Account.Deposit;
            if (balanceAfterPurchased == 0)
            {
                transaction.Commit();
                return;
            }

            if (!CanReturnChange(balanceAfterPurchased))
            {
                var moneyRefunded = RefundPurchase(transaction);
                transaction.RecordChange(moneyRefunded);
                transaction.Rollback();
            }
            else
            {
                var change = ReturnChange(balanceAfterPurchased);
                transaction.RecordChange(change);
                transaction.Commit();
            }
        }

        private Money RefundPurchase(Transaction transaction)
        {
            foreach (var item in transaction.PurchasedItems)
            {
                var product = GetProduct(item.ProductId);
                product.Add(item.AmountOfProducts);
            }

            var moneyToRefund = ReturnChange(transaction.Total);
            transaction.Account.Refund(transaction.Total);

            return moneyToRefund;
        }

        private Money ReturnChange(decimal amount)
        {
            var change = Money.Distribute(amount);
            Money = Money.Substract(change);
            return change;
        }

        public Product? GetProduct(string productId) => 
            Products.SingleOrDefault(product => product.Id == productId);

        public Product? UnloadProduct(string productId)
        {
            var product = Products.SingleOrDefault(product => product.Id == productId);
            if(product != null)
            {
                _products.Remove(product);
            }
            return product;
        }

        public decimal MakeDeposit(string accountId, Money money)
        {
            var account = GetAccount(accountId);
            if (account == null)
            {
                throw new InvalidOperationException($"No Account with Id : {accountId}");
            }
            account.MakeDeposit(money);
            Money = Money.Add(money);

            return account.Deposit;
        }

        public void CancelDeposit(string accountId)
        {
            var account = GetAccount(accountId);
            if (account == null)
            {
                throw new InvalidOperationException($"No Account with Id : {accountId}");
            }
            ReturnChange(account.Deposit);
            account.Debit(account.Deposit);
        }

        public VendingMachineAccount? GetAccount(string accountId) => 
            Accounts.SingleOrDefault(account => account.Id == accountId);

        public bool CanReturnChange(decimal amount)
        {
            var change = Money.Distribute(amount);
            return change.Value == amount;
        }

    }
}
