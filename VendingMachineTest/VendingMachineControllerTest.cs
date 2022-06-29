using FluentAssertions;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Vending.Machine.Data;
using Vending.Machine.Domain.UserAccountManagement;
using Vending.Machine.Web.Api.ViewModels;

namespace VendingMachineTest
{
    public class VendingMachineControllerTest : IClassFixture<TestingWeAppFactory<Program>>
    {
        private readonly HttpClient _httpClient;

        public VendingMachineControllerTest(TestingWeAppFactory<Program> factory)
            => _httpClient = factory.CreateClient();

        [Fact]
        public async Task Deposit_Test()
        {
            var deposit = new MoneyDto
            {
                FiveCent = 0,
                TenCent = 1,
                TwentyCent = 0,
                FiftyCent = 0,
                HundredCent = 0
            };

            var depositJson = JsonConvert.SerializeObject(deposit);
            var requestContent = new StringContent(depositJson, Encoding.UTF8, "application/json");

            var authToken = await GetLoginToken(UserRole.Buyer);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            var response = await _httpClient.PostAsync("/api/VendingMachine/Deposit", requestContent);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var balance = JsonConvert.DeserializeObject<DepositDto>(responseString);

            var expected = 0.6m;
            balance.Deposit.Should().Be(expected);
        }

        [Fact]
        public async Task Buy_Test()
        {
            var productId = "productId";
            var amountOfProducts = 1;

            var query = $"/api/VendingMachine/Buy?productId={productId}&amountOfProducts={amountOfProducts}";
            
            var authToken = await GetLoginToken(UserRole.Buyer);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",authToken);
            var response = await _httpClient.PostAsync(query, null);

            response.EnsureSuccessStatusCode();
        }

        private async Task<string> GetLoginToken(UserRole buyer)
        {
            var loginDto = new UserLoginDto
            {
                Username = "buyer",
                Password = "test123"
            };

            var login = JsonConvert.SerializeObject(loginDto);
            var requestContent = new StringContent(login, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/Authentication/login", requestContent);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();

        }
    }
}
