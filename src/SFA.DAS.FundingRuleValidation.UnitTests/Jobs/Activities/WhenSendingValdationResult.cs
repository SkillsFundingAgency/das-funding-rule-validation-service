using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.FundingRuleValidation.Jobs.Activities;
using SFA.DAS.FundingRuleValidation.Jobs.Core;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Activities;

public class WhenSendingValdationResult
{
    [Test, MoqAutoData]
    public async Task Then_The_Result_Is_Sent(
        Mock<FunctionContext> fakeContext,
        ValidateLearnerResult result)
    {
        // arrange
        var sender = new Mock<ServiceBusSender>();
        var client = new Mock<ServiceBusClient>();

        client
            .Setup(x => x.CreateSender(GlobalConstants.OutgoingQueueName))
            .Returns(sender.Object);

        ServiceBusMessage? capturedMessage = null;
        sender
            .Setup(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceBusMessage, CancellationToken>((x, _) => capturedMessage = x)
            .Returns(Task.CompletedTask);

        var sut = new SendValidationResultActivity(client.Object);
        
        // act
        await sut.Run(result, fakeContext.Object);

        // assert
        capturedMessage.Should().NotBeNull();
        var expectedMessage = JsonSerializer.Deserialize<ValidateLearnerResult>(capturedMessage.Body.ToString());
        expectedMessage.Should().BeEquivalentTo(result);
    }
}