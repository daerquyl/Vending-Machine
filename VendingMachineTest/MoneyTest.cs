using FluentAssertions;
using Vending.Machine.Domain.Core;

namespace VendingMachineTest

{
    public class MoneyTest
    {
        [Theory]
        [InlineData(1,0,0,0,0, 0.05)]
        [InlineData(0,1,0,0,0, 0.1)]
        [InlineData(0,0,1,0,0, 0.2)]
        [InlineData(0,0,0,1,0, 0.5)]
        [InlineData(0,0,0,0,1, 1)]
        public void Should_Have_Value(int fiveCent, int tenCent, int twentyCent, int fiftyCent, int hundredCent, decimal expected)
        {
            var money = new Money(fiveCent, tenCent, twentyCent, fiftyCent, hundredCent);
            money.Value.Should().Be(expected, "Money value is not correctly calculated");
        }

        [Theory]
        [InlineData(-1, 0, 0, 0, 0)]
        [InlineData(0, -1, 0, 0, 0)]
        [InlineData(0, 0, -1, 0, 0)]
        [InlineData(0, 0, 0, -1, 0)]
        [InlineData(0, 0, 0, 0, -1)]
        public void Cannot_Have_Negative_Value(int fiveCent, int tenCent, int twentyCent, int fiftyCent, int hundredCent)
        {
            Action action = () => new Money(fiveCent, tenCent, twentyCent, fiftyCent, hundredCent);

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Money cannot be initialized with negative value");
        }

        [Theory]
        [InlineData(1, 0, 0, 0, 0, 2, 2, 3, 4, 5)]
        [InlineData(0, 1, 0, 0, 0, 1, 3, 3, 4, 5)]
        [InlineData(0, 0, 1, 0, 0, 1, 2, 4, 4, 5)]
        [InlineData(0, 0, 0, 1, 0, 1, 2, 3, 5, 5)]
        [InlineData(0, 0, 0, 0, 1, 1, 2, 3, 4, 6)]
        public void Add_Money_Returns_New_Money_With_Right_Values(
            int fiveCent, int tenCent, int twentyCent, int fiftyCent, int hundredCent,
            int expectedFiveCent, int expectedTenCent, int expectedTwentyCent, int expectedFiftyCent, int expectedHundredCent
            )
        {
            var money = new Money(fiveCent, tenCent, twentyCent, fiftyCent, hundredCent);
            var moneyToAdd = new Money(1, 2, 3, 4, 5);

            var newMoney = money.Add(moneyToAdd);

            newMoney.Should().Be(new Money(expectedFiveCent, expectedTenCent, expectedTwentyCent, expectedFiftyCent, expectedHundredCent));
        }

        [Theory]
        [InlineData(1, 2, 3, 4, 5, 0, 1, 2, 3, 4)]
        [InlineData(1, 1, 1, 1, 1, 0, 0, 0, 0, 0)]
        public void Substract_Money_Returns_New_Money_With_Right_Values(
    int fiveCent, int tenCent, int twentyCent, int fiftyCent, int hundredCent,
    int expectedFiveCent, int expectedTenCent, int expectedTwentyCent, int expectedFiftyCent, int expectedHundredCent
    )
        {
            var money = new Money(fiveCent, tenCent, twentyCent, fiftyCent, hundredCent);
            var moneyToSubstract = new Money(1, 1, 1, 1, 1);

            var newMoney = money.Substract(moneyToSubstract);

            newMoney.Should().Be(new Money(expectedFiveCent, expectedTenCent, expectedTwentyCent, expectedFiftyCent, expectedHundredCent));
        }

        [Theory]
        [InlineData(1.7, 1.7)]
        [InlineData(0.2, 0.2)]
        [InlineData(0.75, 0.75)]
        [InlineData(3.55, 3.25)]
        public void Distribute_Returns_Money_With_Value_Less_Or_Equivalent_To_AmountDistributed(decimal amountDistributed, decimal amountExpected)
        {
            var money = new Money(5, 4, 3, 2, 1);

            var distribution = money.Distribute(amountDistributed);

            distribution.Value.Should().Be(amountExpected);
        }
    }
}