using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vending.Machine.Domain.Core;
using Vending.Machine.Domain.Core.Repository;
using Vending.Machine.Domain.UserAccountManagement.Repository;
using Vending.Machine.Web.Api.ViewModels;

namespace Vending.Machine.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendingMachineController : ControllerBase
    {
        private IVendingMachineRepository _repository;
        private readonly IUserRepository _userRepository;
        private const string AccountNotFound = "Account not found";
        private const string AccountOrDepositNotFound = "Account or Product not found";

        public VendingMachineController(IVendingMachineRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        // Get : /api/VendingMachine
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<VendingMachineDto>> Get()
        {
            var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vendingMachineDto = VendingMachineDto.ForAccount(
                    _repository.GetVendingMachine(),
                    buyerId
                );
            return Ok(vendingMachineDto);
        }

        // Post : /api/VendingMachine/Deposit
        [HttpPost("Deposit")]
        [Authorize(Roles = "Buyer")]
        public async Task<ActionResult<DepositDto>> MakeDeposit(MoneyDto moneyDto)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var balance = 0m;
            try
            {
                balance = _repository.GetVendingMachine().MakeDeposit(accountId, moneyDto.ToMoney());
                await _repository.SaveChanges();
                var account = _repository.GetVendingMachine().GetAccount(accountId);
                await _userRepository.UpdateUserDeposit(accountId, account.Deposit);//Should be Handle using Domain Event

            }
            catch (InvalidOperationException ex)
            {
                return Forbid(AccountNotFound);
            }
            return Ok(new DepositDto(balance));
        }

        // Post : /api/VendingMachine/Buy
        [HttpPost("Buy")]
        [Authorize(Roles = "Buyer")]

        public async Task<ActionResult<TransactionDto>> BuyProduct(string productId, int amountOfProducts)
        {
            Transaction transaction;
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                transaction = _repository.GetVendingMachine().BuyProduct(accountId, productId, amountOfProducts);
                await _repository.SaveChanges();
                await _userRepository.UpdateUserDeposit(accountId, 0);//Should be Handle using Domain Event
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(AccountOrDepositNotFound);
            }
            return Ok(TransactionDto.FromTransaction(transaction));
        }

        // Post : /api/VendingMachine/Deposit/Reset
        [HttpPost("Deposit/Reset")]
        [Authorize(Roles = "Buyer")]

        public async Task<ActionResult<DepositDto>> ResetDeposit()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                _repository.GetVendingMachine().CancelDeposit(accountId);
                await _repository.SaveChanges();
                await _userRepository.UpdateUserDeposit(accountId, 0);//Should be Handle using Domain Event

            }
            catch (InvalidOperationException ex)
            {
                return Forbid(AccountNotFound);
            }
            return Ok(new DepositDto(0m));
        }
    }
}
