using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Vending.Machine.Domain.UserAccountManagement;
using Vending.Machine.Web.Api.ViewModels;
using Vending.Machine.Data;

namespace VendingMachineTest
{
    public class UserControllerTest: IClassFixture<TestingWeAppFactory<Program>>
    {
        private readonly HttpClient _httpClient;

        public UserControllerTest(TestingWeAppFactory<Program> factory)
            => _httpClient = factory.CreateClient();
        
        [Fact]
        public async Task Create_And_Get_User_Test()
        {
            var username = "username";
            var password = "password";
            var role = UserRole.Seller;
            var userDto = new CreateUserDto
            {
                Username = username,
                Password = password,
                Role = role.ToString()
            };


            var userDtoJson = JsonConvert.SerializeObject(userDto);
            var requestContent = new StringContent(userDtoJson, Encoding.UTF8, "application/json");
            var createResponse = await _httpClient.PostAsync("/api/User", requestContent);

            createResponse.EnsureSuccessStatusCode();

            var responseString = await createResponse.Content.ReadAsStringAsync();
            var createdUser = JsonConvert.DeserializeObject<UserDto>(responseString);

            var getResponse = await _httpClient.GetAsync($"/api/User/{createdUser.Id}");
            responseString = await getResponse.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserDto>(responseString);

            user.Id.Should().Be(createdUser.Id);
            user.Username.Should().Be(username);
            user.Deposit.Should().Be(0m);
            user.Role.Should().Be(UserRole.Seller.ToString());
        }
    }
}
