using FluentAssertions;
using Vending.Machine.Domain.Core;

namespace VendingMachineTest

{
    public class VendingMachineAccountTest
    {
        [Fact]
        public void Adding_Money_Increase_Deposit_Accordingly()
        {
            var vendingMachineAccount = new VendingMachineAccount();

            vendingMachineAccount.MakeDeposit(Money.FiftyCentEuros);
            vendingMachineAccount.MakeDeposit(Money.OneEuro);

            vendingMachineAccount.Deposit.Should().Be(1.5m);
        }

        [Theory]
        [InlineData(2, 0, 0, 0, 0)]
        [InlineData(0, 2, 0, 0, 0)]
        [InlineData(0, 0, 2, 0, 0)]
        [InlineData(0, 0, 0, 2, 0)]
        [InlineData(0, 0, 0, 0, 2)]
        [InlineData(1, 0, 0, 0, 1)]
        public void Cannot_Make_Deposit_With_Unauthorized_Coin(int fiveCent, int tenCent, int twentyCent, int fiftyCent, int hundredCent)
        {
            var vendingMachineAccount = new VendingMachineAccount();

            var unexpectedCoin = new Money(fiveCent, tenCent, twentyCent, fiftyCent, hundredCent);
            Action addDeposit = () => vendingMachineAccount.MakeDeposit(unexpectedCoin);

            addDeposit.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Only 5, 10, 20, 50, 100 cent can be inserted at time");
        }

        [Theory]
        [InlineData(0.2, 0.8)]
        [InlineData(1, 0)]
        [InlineData(1.2, 1)]
        public void Debit_Only_When_Sufficent_Deposit(decimal debit, decimal balance)
        {
            var deposit = Money.OneEuro;
            var vendingMachineAccount = new VendingMachineAccount();
            vendingMachineAccount.MakeDeposit(deposit);

            vendingMachineAccount.Debit(debit);

            vendingMachineAccount.Deposit.Should().Be(balance);
        }

        [Theory]
        [InlineData(0.7, true)]
        [InlineData(1, true)]
        [InlineData(1.2, false)]
        public void Can_Debit_Only_When_Sufficent_Deposit(decimal debit, bool expected)
        {
            var deposit = Money.OneEuro;
            var vendingMachineAccount = new VendingMachineAccount();
            vendingMachineAccount.MakeDeposit(deposit);

            var canDebit = vendingMachineAccount.CanDebit(debit);

            canDebit.Should().Be(expected);
        }

        [Fact]
        public void Cannot_Debit_Negative_Amount()
        {
            var deposit = Money.OneEuro;
            var vendingMachineAccount = new VendingMachineAccount();
            vendingMachineAccount.MakeDeposit(deposit);

            var amountToDebit = -5;
            var action = () => vendingMachineAccount.Debit(amountToDebit);

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Not a valid debit operation");
        }

    }
}