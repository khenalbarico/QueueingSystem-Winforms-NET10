using AwesomeAssertions;
using LogicLibrary1.AuthHandler1;
using LogicLibrary1.Models1.Auth1;
using LogicLibrary1.Models1.User1;
using TestProject1.TestTools;
using Xunit.Abstractions;
using static LogicLibrary1.Models1.Constants1;

namespace TestProject1.AuthHandlerFacts1;

public class AuthenticationFacts1 (ITestOutputHelper _ctx)
{
    [Fact] public async Task Create_NewAccount_Should_Create_Account_Successfully()
    {
        //Arrange
        var _sut = _ctx.Get<Authentication1>();

        var payload = new UserInfoModels1
        {
            Email = "testemail4@test.com",
            Password = "testpassword",
            FirstName = "Test Name",
            LastName = "Test LastName",
            Age = 30,
            PhoneNumber = 1234567890
        };

        //Act
        var result = await _sut.OnCreateNewAccount(payload);

        //Assert
        result.Should().BeTrue();
    }
    [Fact] public async Task Login_Should_Return_UserInfo()
    {
        //Arrange
        var _sut = _ctx.Get<Authentication1>();

        var expected = new UserInfoModels1
        {
            UserId = "TT-0001",
            Email = "testemail2@test.com",
            Password = "testpassword",
            FirstName = "Test Name",
            LastName = "Test LastName",
            UserRole = UserRole.Member,
            Age = 30,
            PhoneNumber = 1234567890,
            CreatedAt = new DateTime(2026, 2, 22, 15, 24, 0)
        };

        var payload = new LoginModels1
        {
            Email = "testemail2@test.com",
            Password = "testpassword"
        };

        //Act
        var result = await _sut.OnLoginAsync(payload);

        //Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact] public async Task Login_Should_Get_Current_UserId()
    {
        //Arrange
        var _sut = _ctx.Get<Authentication1>();

        var payload = new LoginModels1
        {
            Email = "testemail2@test.com",
            Password = "testpassword"
        };

        //Act
        var act1 = await _sut.OnLoginAsync(payload);
        var act2 = _sut.GetCurrentUserId();      

        //Assert
        act2.Should().Be(act1.UserId);
    }
}

