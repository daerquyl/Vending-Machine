using FluentAssertions;
using Vending.Machine.Domain.Core;

namespace VendingMachineTest
{
    public class VendorMachineTest
    {
        [Fact]
        public void Can_Load_Product_In()
        {
            var product = new Product("Twist", 0.2m, 10, Guid.NewGuid().ToString());
            var vendingMachine = new VendingMachine();

            vendingMachine.LoadProduct(product);

            vendingMachine.Products.Should().Contain(product);
        }

        [Fact]
        public void Cannot_Load_Product_With_Negatif_Cost_Or()
        {
            var product = new Product("Twist", -0.2m, 10, Guid.NewGuid().ToString());
            var vendingMachine = new VendingMachine();

            var loadProduct = () => vendingMachine.LoadProduct(product);

            loadProduct.Should().Throw<InvalidOperationException>()
                .WithMessage("Product must have a positive cost");
        }

        [Fact]
        public void Cannot_Load_Product_With_Zero_Availability()
        {
            var product = new Product("Twist", 0.2m, 0, Guid.NewGuid().ToString());
            var vendingMachine = new VendingMachine();

            var loadProduct = () => vendingMachine.LoadProduct(product);

            loadProduct.Should().Throw<InvalidOperationException>()
                .WithMessage("No product to load");
        }

        [Fact]
        public void Can_Register_New_Account()
        {
            var account = new VendingMachineAccount();
            var vendingMachine = new VendingMachine();

            vendingMachine.RegisterAccount(account);
            vendingMachine.Accounts.Should().Contain(account);
        }

        [Fact]
        public void Make_Deposit_Increase_Money_Inside()
        {
            var vendingMachine = new VendingMachine();
            var account = new VendingMachineAccount();

            vendingMachine.RegisterAccount(account);
            vendingMachine.MakeDeposit(account.Id, Money.FiftyCentEuros);
            vendingMachine.MakeDeposit(account.Id, Money.FiftyCentEuros);

            var expectedMoney = new Money(0, 0, 0, 2, 0);
            vendingMachine.Money.Should().Be(expectedMoney);
        }

        [Fact]
        public void Cancel_Deposit_Decrease_Money_Inside_And_Empty_Account()
        {
            var vendingMachine = new VendingMachine();
            var account = new VendingMachineAccount();

            vendingMachine.RegisterAccount(account);
            vendingMachine.MakeDeposit(account.Id, Money.FiftyCentEuros);
            vendingMachine.MakeDeposit(account.Id, Money.FiftyCentEuros);

            vendingMachine.CancelDeposit(account.Id);

            var expectedMoney = Money.None;
            vendingMachine.Money.Should().Be(expectedMoney);
            account.Deposit.Should().Be(0);
        }

        [Theory]
        [InlineData(0.2, 10, 5, 5, 0.05, 20, 20, 0, 0)]
        [InlineData(0.2, 10, 10, 0, 0.05, 20, 1, 20, 0)]
        [InlineData(0.2, 10, 11, 10, 0.05, 20, 20, 0, 1)]
        public void Buy_Only_Available_Products_Only_With_Sufficent_Deposit(
            decimal product1Cost, int product1AmountAvailable, int amountOfProduct1ToBuy, int expectedAvailableAmmount1,
            decimal product2Cost, int product2AmountAvailable, int amountOfProduct2ToBuy, int expectedAvailableAmmount2,
            decimal expectedDeposit
            )
        {
            var vendingMachine = new VendingMachine();

            var product1 = new Product("productName1", product1Cost, product1AmountAvailable, "productId1");
            var product2 = new Product("productName2", product2Cost, product2AmountAvailable, "productId2");
            vendingMachine.LoadProduct(product1);
            vendingMachine.LoadProduct(product2);

            var account = new VendingMachineAccount();
            vendingMachine.RegisterAccount(account);
            vendingMachine.MakeDeposit(account.Id, Money.OneEuro);
            vendingMachine.MakeDeposit(account.Id, Money.OneEuro);

            var buyOrder = new List<OrderItem>()
            {
                new OrderItem(product1.Id, amountOfProduct1ToBuy, product1.Cost),
                new OrderItem(product2.Id, amountOfProduct2ToBuy, product2.Cost),
            };

            var transaction = vendingMachine.BuyProducts(account.Id, buyOrder);
            var product1PurchasedTotal = transaction.PurchasedItems.Where(item => item.Product.Id == product1.Id)
                ?.Select(item => item.AmountOfProducts)?.FirstOrDefault();
            var product2PurchasedTotal = transaction.PurchasedItems.Where(item => item.Product.Id == product2.Id)
                ?.Select(item => item.AmountOfProducts)?.FirstOrDefault();

            account.Deposit.Should().Be(expectedDeposit, $"Deposit should be reduced by {transaction.Total} after buying");
            vendingMachine.GetProduct(product1.Id).AmountAvailable.Should().Be(expectedAvailableAmmount1, $"Product available amount should be reduced by {product1PurchasedTotal ?? 0} after buying");
            vendingMachine.GetProduct(product2.Id).AmountAvailable.Should().Be(expectedAvailableAmmount2, $"Product available amount should be reduced by {product2PurchasedTotal ?? 0} after buying");
        }

        [Theory]
        [InlineData(15, 16)]
        [InlineData(1, 10)]
        [InlineData(15, 20)]
        public void Cannot_Buy_UnavailableProduct(int amountAvailable, int amountToBuy)
        {
            var vendingMachine = new VendingMachine();

            var product = new Product("productName1", 0.5m, amountAvailable, "productId1");
            vendingMachine.LoadProduct(product);

            var account = new VendingMachineAccount();
            vendingMachine.RegisterAccount(account);
            vendingMachine.MakeDeposit(account.Id, Money.OneEuro);

            var buyOrder = new List<OrderItem>(){ new OrderItem(product.Id, amountToBuy, product.Cost) };

            vendingMachine.BuyProducts(account.Id, buyOrder);

            var expectedDeposit = 1;
            account.Deposit.Should().Be(expectedDeposit, "Account deposit should not be reduced when no product can't be purchased");
            vendingMachine.GetProduct(product.Id).AmountAvailable.Should().Be(amountAvailable, $"Product available amount should not be reduced when there is no sufficent amount to buy");
        }

        [Fact]
        public void Cannot_Buy_Product_If_Insufficent_Money_For_Change()
        {
            var vendingMachine = new VendingMachine();

            var amountAvailable = 10;
            var cost = 0.7m;
            var product = new Product("productName1", cost, amountAvailable, "productId1");
            vendingMachine.LoadProduct(product);

            var account = new VendingMachineAccount();
            vendingMachine.RegisterAccount(account);
            //Not enough coin to return 0.3 euros
            vendingMachine.MakeDeposit(account.Id, Money.OneEuro);

            var buyOrder = new List<OrderItem>(){ new OrderItem(product.Id, 1, cost) };

            var transaction = vendingMachine.BuyProducts(account.Id, buyOrder);

            account.Deposit.Should().Be(Money.OneEuro.Value, "Account deposit should not be reduced when no change can't be returned");
            vendingMachine.GetProduct(product.Id).AmountAvailable.Should().Be(amountAvailable, "Product available amount should not be updated when no change can't be returned");
            transaction.Status.Should().Be(TransactionStatus.Canceled);
        }

        [Fact]
        public void Return_change_After_Purchase_When_Deposit_In_Account_Is_Not_Empty()
        {
            var vendingMachine = new VendingMachine();

            var product = new Product("productName1", 0.2m, 10, "productId1");
            vendingMachine.LoadProduct(product);

            var account1 = new VendingMachineAccount();
            var account2 = new VendingMachineAccount();
            vendingMachine.RegisterAccount(account1);
            vendingMachine.RegisterAccount(account2);
            vendingMachine.MakeDeposit(account1.Id, Money.OneEuro);
            vendingMachine.MakeDeposit(account1.Id, Money.FiftyCentEuros);
            vendingMachine.MakeDeposit(account1.Id, Money.TwentyCentEuros);
            vendingMachine.MakeDeposit(account2.Id, Money.TenCentEuros);
            vendingMachine.MakeDeposit(account2.Id, Money.FiveCentEuros);

            //After this Deposit value is 1.5m for account 1
            var buyOrder = new List<OrderItem>() { new OrderItem(product.Id, 1, 0.2m) };

            var transaction = vendingMachine.BuyProducts(account1.Id, buyOrder);

            var expectedChangeValue = 1.5m;
            var expectedVendingMachineMoneyValue = 0.35m;
            vendingMachine.Money.Value.Should().Be(expectedVendingMachineMoneyValue, "Vending machine money inside should be reduced after change returned");
            transaction.Change.Value.Should().Be(expectedChangeValue, "Wrong change is returned after purchased");
        }

        [Theory]
        [InlineData(3.55, false)]
        [InlineData(1.5, true)]
        [InlineData(0.4, false)]
        [InlineData(0.75, true)]
        [InlineData(1.30, true)]
        public void Can_Return_Change_If_sufficent_Coins(decimal amountDistributed, bool expected)
        {
            var vendingMachine = new VendingMachine();

            var account = new VendingMachineAccount();
            vendingMachine.RegisterAccount(account);
            vendingMachine.MakeDeposit(account.Id, Money.OneEuro);
            vendingMachine.MakeDeposit(account.Id, Money.FiftyCentEuros);
            vendingMachine.MakeDeposit(account.Id, Money.FiftyCentEuros);
            vendingMachine.MakeDeposit(account.Id, Money.TwentyCentEuros);
            vendingMachine.MakeDeposit(account.Id, Money.TenCentEuros);
            vendingMachine.MakeDeposit(account.Id, Money.FiveCentEuros);

            var canReturnChange = vendingMachine.CanReturnChange(amountDistributed);

            canReturnChange.Should().Be(expected, "There is sufficent money inside vending machine to return change");
        }
    }
}
