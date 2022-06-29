using Vending.Machine.Domain.Core;

namespace Vending.Machine.Web.Api.ViewModels
{
    public class MoneyDto
    {
        public int FiveCent { get; set; }
        public int TenCent { get; set; }
        public int TwentyCent { get; set; }
        public int FiftyCent { get; set; }
        public int HundredCent { get; set; }

        public static MoneyDto FromMoney(Money money)
        {
            if(money == null)
            {
                return new MoneyDto
                {
                    FiveCent = 0,
                    TenCent = 0,
                    TwentyCent = 0,
                    FiftyCent = 0,
                    HundredCent = 0,
                };
            }

            return new MoneyDto
            {
                FiveCent = money.FiveCent,
                TenCent = money.TenCent,
                TwentyCent = money.TwentyCent,
                FiftyCent = money.FiftyCent,
                HundredCent = money.HundredCent,
            };
        }
        public Money ToMoney() =>
            new Money(FiveCent, TenCent, TwentyCent, FiftyCent, HundredCent);
    }
}
