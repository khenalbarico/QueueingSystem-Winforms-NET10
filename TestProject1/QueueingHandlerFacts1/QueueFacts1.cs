using AwesomeAssertions;
using LogicLibrary1.AuthHandler1;
using LogicLibrary1.Models1.Auth1;
using LogicLibrary1.Models1.Queue1;
using LogicLibrary1.QueueingHandler1;
using TestProject1.TestTools;
using Xunit.Abstractions;
using static LogicLibrary1.Models1.Constants1;

namespace TestProject1.QueueingHandlerFacts1;

public class QueueFacts1(ITestOutputHelper _ctx)
{
    [Fact] public async Task Queueing_Event_Should_Queue_UserId()
    {
        //Arrange
        var loginPayload = new LoginModels1
        {
            Email = "testemail4@test.com",
            Password = "testpassword"
        };

        var queuePayload = new QueueModels1
        {
            QueueService = QueueService.Enroll
        };

        var _sut2 = _ctx.Get<Authentication1>();

        var _sut = _ctx.Get<Queue1>();

        //Act
        _ = await _sut2.OnLoginAsync(loginPayload);
        var act2 = await _sut.EnqueueAsync(queuePayload);

        //Assert
        act2.Should().BeTrue();
    }

    [Fact] public async Task Dequeueing_Event_Should_Dequeue_UserId()
    {
        //Arrange
        var loginPayload = new LoginModels1
        {
            Email = "testemail2@test.com",
            Password = "testpassword"
        };

        var queuePayload = new QueueModels1
        {
            QueueId = "ERL-0003"
        };

        var _sut2 = _ctx.Get<Authentication1>();

        var _sut = _ctx.Get<Queue1>();

        //Act
        _= await _sut2.OnLoginAsync(loginPayload);
        var act2 = await _sut.DequeueAsync(queuePayload);

        //Assert
        act2.Should().BeTrue();
    }

    [Fact] public async Task Global_Display_All_Queues_Has_Count()
    {
        //Arrange
        var _sut = _ctx.Get<Queue1>();

        //Act
        var act = await _sut.DisplayAllQueuesAsync();

        //Assert
        act.Should().NotBeEmpty();
    }

}
