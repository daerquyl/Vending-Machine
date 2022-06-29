namespace Vending.Machine.Web.Api.ViewModels
{
    public class DepositDto
    {
        public decimal Deposit { get; set; }
        public DepositDto(decimal deposit)
        {
            Deposit = deposit;
        }
    }
}
