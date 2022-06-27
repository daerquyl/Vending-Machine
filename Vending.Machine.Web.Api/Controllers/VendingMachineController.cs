using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vending.Machine.Domain.Core;
using Vending.Machine.Domain.Core.Repository;
using Vending.Machine.Domain.UserAccountManagement.Repository;

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

        // Post : /api/VendingMachine/Deposit
        [HttpPost("Deposit")]
        public async Task<IActionResult> MakeDeposit(Money money)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                _repository.GetVendingMachine().MakeDeposit(accountId, money);
                await _repository.SaveChanges();
                var account = _repository.GetVendingMachine().GetAccount(accountId);
                await _userRepository.UpdateUserDeposit(accountId, account.Deposit);//Should be Handle using Domain Event

            }
            catch (InvalidOperationException ex)
            {
                return Forbid(AccountNotFound);
            }
            return Ok();
        }

        // Post : /api/VendingMachine/Buy
        [HttpPost("Buy")]
        public async Task<ActionResult<Transaction>> BuyProduct(string productId, int amountOfProducts)
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
            return Ok(transaction);
        }

        // Post : /api/VendingMachine/Deposit/Reset
        [HttpPost("Deposit/Reset")]
        public async Task<IActionResult> ResetDeposit()
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
            return Ok();
        }
    }
}
