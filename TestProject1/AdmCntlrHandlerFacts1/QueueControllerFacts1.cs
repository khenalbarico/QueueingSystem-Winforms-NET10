using LogicLibrary1.AdmCntlrHandler1;
using LogicLibrary1.AuthHandler1;
using LogicLibrary1.Models1.Auth1;
using LogicLibrary1.Models1.Services.Consultation;
using TestProject1.TestTools;
using Xunit.Abstractions;
using static LogicLibrary1.Models1.Constants1;

namespace TestProject1.AdmCntlrHandlerFacts1;

public class QueueControllerFacts1 (ITestOutputHelper _ctx)
{
    [Fact] public async Task Admin_Should_Move_Queue_To_Processing()
    {
        //Arrange
        var _sut = _ctx.Get<QueueController1>();

        var _sut2 = _ctx.Get<Authentication1>();

        var loginPayload = new LoginModels1
        {
            Email = "testemail4@test.com",
            Password = "testpassword"
        };

        //Act
        var authResult = await _sut2.OnLoginAsync(loginPayload);
        var sutResult = await _sut.MoveToProcessingAsync("ERL-0002");

        //Assert
        //ERL-0002 should be in Processing Status
    }

    [Fact] public async Task Admin_Should_Move_Queue_To_Complete()
    {
        //Arrange
        var _sut = _ctx.Get<QueueController1>();

        var _sut2 = _ctx.Get<Authentication1>();

        var loginPayload = new LoginModels1
        {
            Email = "testemail4@test.com",
            Password = "testpassword"
        };

        //Act
        var authResult = await _sut2.OnLoginAsync(loginPayload);
        var sutResult = await _sut.MoveToCompleteAsync("ERL-0002");

        //Assert
        //ERL-0002 should be in Completed Status
    }

    [Fact] public async Task Admin_Should_Move_Queue_Back_To_Pending()
    {
        //Arrange
        var _sut = _ctx.Get<QueueController1>();

        var _sut2 = _ctx.Get<Authentication1>();

        var loginPayload = new LoginModels1
        {
            Email = "testemail4@test.com",
            Password = "testpassword"
        };

        //Act
        var authResult = await _sut2.OnLoginAsync(loginPayload);
        var sutResult = await _sut.SetBackAsync("ERL-0002", Status.Pending);

        //Assert
        //ERL-0002 should be in Pending Status
    }

    [Fact] public async Task Admin_Should_Create_Professor()
    {
        //Arrange
        var payload = new ProffesorModels1
        {
            FirstName = "TestName",
            LastName = "TestLastName",
            Subject = "Information Technoloy"
        };

        var _sut = _ctx.Get<ServicesController1>();

        var _sut2 = _ctx.Get<Authentication1>();

        var loginPayload = new LoginModels1
        {
            Email = "testemail4@test.com",
            Password = "testpassword"
        };

        //Act
        var authResult = await _sut2.OnLoginAsync(loginPayload);
        var profResult = await _sut.AddProfessorAsync(payload);

        //Assert
        //payload should be created in TeachersDatabase.xlsx
    }

    [Fact] public async Task Admin_Should_Delete_Professor()
    {
        //Arrange
        var _sut = _ctx.Get<ServicesController1>();

        var _sut2 = _ctx.Get<Authentication1>();

        var loginPayload = new LoginModels1
        {
            Email = "testemail4@test.com",
            Password = "testpassword"
        };

        //Act
        var authResult = await _sut2.OnLoginAsync(loginPayload);
        var profResult = await _sut.DeleteProfessorAsync("TT1-INFO");

        //Assert
        //payload should be deleted in TeachersDatabase.xlsx
    }
}
