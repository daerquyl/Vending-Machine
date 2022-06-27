using Vending.Machine.Domain.Common;

namespace Vending.Machine.Domain.Core
{
    public class VendingMachineAccount: Entity
    {
        public VendingMachineAccount() { }

        public VendingMachineAccount(string id)
        {
            Id = id;
        }

        public decimal Deposit { get; private set; }

        private static List<Money> AuthorizedCoins => new List<Money>()
        {
            Money.FiveCentEuros, Money.TenCentEuros, Money.TwentyCentEuros, Money.FiftyCentEuros, Money.OneEuro
        };

        public void MakeDeposit(Money money)
        {
            if (!AuthorizedCoins.Contains(money))
            {
                throw new InvalidOperationException("Only 5, 10, 20, 50, 100 cent can be inserted at time");
            }
            Deposit += money.Value;
        }

        public void Refund(decimal amount)
        {
            Deposit += amount;
        }

        public void Debit(decimal debit)
        {
            if (debit < 0)
            {
                throw new InvalidOperationException("Not a valid debit operation");
            }

            if (CanDebit(debit))
            {
                Deposit -= debit;
            }
        }

        public bool CanDebit(decimal debit)
        {
            return debit <= Deposit;
        }
    }
}