namespace Vending.Machine.Domain.Core
{
    public record Money
    {
        public static readonly Money FiveCentEuros = new Money(1, 0, 0, 0, 0);
        public static readonly Money TenCentEuros = new Money(0, 1, 0, 0, 0);
        public static readonly Money TwentyCentEuros = new Money(0, 0, 1, 0, 0);
        public static readonly Money FiftyCentEuros = new Money(0, 0, 0, 1, 0);
        public static readonly Money OneEuro = new Money(0, 0, 0, 0, 1);

        public static readonly Money None = new Money(0, 0, 0, 0, 0);

        public Money() { }
        public Money(int fiveCent, int tenCent, int twentyCent, int fiftyCent, int hundredCent)
        {
            if(fiveCent < 0 
                || tenCent < 0
                || twentyCent < 0
                || fiftyCent < 0
                || hundredCent < 0)
            {
                throw new InvalidOperationException("Money cannot be initialized with negative value");
            }
            FiveCent = fiveCent;
            TenCent = tenCent;
            TwentyCent = twentyCent;
            FiftyCent = fiftyCent;
            HundredCent = hundredCent;

            _value = (FiveCent * 5 +
                TenCent * 10 +
                TwentyCent * 20 +
                FiftyCent * 50 +
                HundredCent * 100) / 100m;
        }

        private decimal _value = 0;
        public decimal Value => _value;

        public int FiveCent { get; }
        public int TenCent { get; }
        public int TwentyCent { get; }
        public int FiftyCent { get; }
        public int HundredCent { get; }

        public Money Add(Money moneyToAdd)
        {
            var fiveCent = FiveCent + moneyToAdd.FiveCent;
            var tenCent = TenCent + moneyToAdd.TenCent;
            var twentyCent = TwentyCent + moneyToAdd.TwentyCent;
            var fiftyCent = FiftyCent + moneyToAdd.FiftyCent;
            var hundredCent = HundredCent + moneyToAdd.HundredCent;

            return new Money(fiveCent, tenCent, twentyCent, fiftyCent, hundredCent);
        }

        public Money Substract(Money moneyToSubstract)
        {
            var fiveCent = FiveCent - moneyToSubstract.FiveCent;
            var tenCent = TenCent - moneyToSubstract.TenCent;
            var twentyCent = TwentyCent - moneyToSubstract.TwentyCent;
            var fiftyCent = FiftyCent - moneyToSubstract.FiftyCent;
            var hundredCent = HundredCent - moneyToSubstract.HundredCent;

            return new Money(fiveCent, tenCent, twentyCent, fiftyCent, hundredCent);
        }

        public Money Distribute(decimal amount)
        {
            var coins = new Dictionary<string, Money>() {
                { nameof(FiveCent),  Money.FiveCentEuros }, 
                { nameof(TenCent),  Money.TenCentEuros }, 
                { nameof(TwentyCent),  Money.TwentyCentEuros }, 
                { nameof(FiftyCent),  Money.FiftyCentEuros }, 
                { nameof(HundredCent),  Money.OneEuro }, 
            }.OrderByDescending(coin => coin.Value.Value);

            var distributedCoins = new Dictionary<string, int>() {
                { nameof(FiveCent),  FiveCent },
                { nameof(TenCent),  TenCent },
                { nameof(TwentyCent), TwentyCent },
                { nameof(FiftyCent), FiftyCent },
                { nameof(HundredCent), HundredCent },
            };

            var remainAmount = amount;
            foreach(var coinDetail in coins)
            {
                var partOfAmount = (int)(remainAmount / coinDetail.Value.Value);
                distributedCoins[coinDetail.Key] = Math.Min(partOfAmount, distributedCoins[coinDetail.Key]);
                remainAmount -= coinDetail.Value.Value * distributedCoins[coinDetail.Key];
            }

            return new Money(
                distributedCoins[nameof(FiveCent)],
                distributedCoins[nameof(TenCent)],
                distributedCoins[nameof(TwentyCent)],
                distributedCoins[nameof(FiftyCent)],
                distributedCoins[nameof(HundredCent)]
            );
        }
    }
}
