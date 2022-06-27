using FluentAssertions;
using Vending.Machine.Domain.Core;

namespace VendingMachineTest
{
    public class ProductTest
    {
        [Theory]
        [InlineData(15, 10, 5)]
        [InlineData(15, 15, 0)]
        [InlineData(15, 20, 15)]
        public void Substract_Amount_Only_When_Sufficent_Available(int amountAvailable, int amountToSubstract, int expected)
        {
            var name = "Mars";
            var cost = 0.2m;
            var sellerId = Guid.NewGuid().ToString();
            var product = new Product(name, cost, amountAvailable, sellerId);

            product.Substract(amountToSubstract);

            product.AmountAvailable.Should().Be(expected, "Product initial amount is not sufficent, but substraction still happen");
        }

        [Fact]
        public void Cannot_Substract_Negative_Amount()
        {
            var name = "Mars";
            var cost = 0.2m;
            var amountAvailable = 15;
            var sellerId = Guid.NewGuid().ToString();
            var product = new Product(name, cost, amountAvailable, sellerId);

            var amountToSubstract = -5;
            var action = () => product.Substract(amountToSubstract);

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Not a valid amount to substract");
        }

        [Theory]
        [InlineData(15, 12, true)]
        [InlineData(15, 15, true)]
        [InlineData(15, 20, false)]
        public void Can_Substract_Only_When_Sufficent_Amount_Available(int amountAvailable, int amountToSubstract, bool expected)
        {
            var name = "Mars";
            var cost = 0.2m;
            var sellerId = Guid.NewGuid().ToString();
            var product = new Product(name, cost, amountAvailable, sellerId);

            var canSubstract = product.CanSubstract(amountToSubstract);

            canSubstract.Should().Be(expected, $"Product initial amount ({amountAvailable}) is not sufficent, then it should not be possibe to substract ({amountToSubstract}) from it");
        }

        [Theory]
        [InlineData(15, 10, 25)]
        [InlineData(0, 15, 15)]
        [InlineData(15, 20, 35)]
        public void Can_Add_To_Amount_Available(int amountAvailable, int amountToAdd, int expected)
        {
            var name = "Mars";
            var cost = 0.2m;
            var sellerId = Guid.NewGuid().ToString();
            var product = new Product(name, cost, amountAvailable, sellerId);

            product.Add(amountToAdd);

            product.AmountAvailable.Should().Be(expected);
        }
    }
}
