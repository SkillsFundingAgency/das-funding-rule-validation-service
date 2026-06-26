using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core.Amqp;
using Azure.Core.Serialization;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Amqp;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;
using SFA.DAS.FundingRuleValidation.Jobs.Endpoints;
using SFA.DAS.FundingRuleValidation.Jobs.Orchestrators;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Endpoints;

public class WhenRunningFundingRuleServiceBusTrigger
{
    [Test, MoqAutoData]
    public async Task Then_Exception_Is_Thrown_If_Invalid_Message_Is_Received(Mock<FunctionContext> fakeContext)
    {
        // arrange
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(new BinaryData("{}"u8.ToArray()));

        // act
        var action = async () => await FundingRuleServiceBusEndpoint.FundingRuleServiceBusTrigger(message, null!, fakeContext.Object);

        // assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }
    
    [Test, MoqAutoData]
    public async Task Then_A_New_Funding_Rules_Orchestration_Is_Scheduled(
        string instanceId,
        ValidateLearnerCommand command,
        Mock<FunctionContext> functionContext,
        Mock<DurableTaskClient> durableClient,
        Mock<IOptions<WorkerOptions>> workerOptions,
        Mock<ObjectSerializer> objectSerializer)
    {
        // arrange
        functionContext
            .Setup(x => x.InstanceServices.GetService(typeof(ILogger<FundingRuleServiceBusEndpoint>)))
            .Returns(new Mock<ILogger<FundingRuleServiceBusEndpoint>>().Object);

        string? capturedTaskName = null;
        ValidateLearnerCommand? capturedCommand = null;
        durableClient
            .Setup(x => x.ScheduleNewOrchestrationInstanceAsync(
                It.IsAny<TaskName>(),
                It.IsAny<object>(),
                It.IsAny<StartOrchestrationOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<TaskName, object?, StartOrchestrationOptions, CancellationToken>((taskName, data, _, _) =>
            {
                capturedTaskName = taskName.Name;
                capturedCommand = data as ValidateLearnerCommand;
            })
            .ReturnsAsync(instanceId);

        // for serializing the response
        workerOptions
            .Setup(x => x.Value)
            .Returns(new WorkerOptions { Serializer = objectSerializer.Object });
        functionContext
            .Setup(x => x.InstanceServices.GetService(typeof(IOptions<WorkerOptions>)))
            .Returns(workerOptions.Object);
        
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(new BinaryData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(command))));
        
        // act
        await FundingRuleServiceBusEndpoint.FundingRuleServiceBusTrigger(message, durableClient.Object, functionContext.Object);

        // assert
        capturedTaskName.Should().Be(nameof(FundingRuleOrchestrator.ApplyFundingRules));
        capturedCommand.Should().NotBeNull();
        capturedCommand.Should().BeEquivalentTo(command);
    }
}