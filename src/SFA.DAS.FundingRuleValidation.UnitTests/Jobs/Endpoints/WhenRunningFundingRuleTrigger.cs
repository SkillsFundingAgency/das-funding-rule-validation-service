using System.Net;
using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;
using SFA.DAS.FundingRuleValidation.Jobs.Endpoints;
using SFA.DAS.FundingRuleValidation.Jobs.Orchestrators;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Endpoints;

public class WhenRunningFundingRuleTrigger
{
    [Test, MoqAutoData]
    public async Task Then_BadRequest_Is_Returned_If_No_Ilr_Is_Submitted(Mock<FunctionContext> fakeContext)
    {
        // arrange
        fakeContext
            .Setup(x => x.InstanceServices.GetService(typeof(ILogger<FundingRuleHttpEndpoint>)))
            .Returns(new Mock<ILogger<FundingRuleHttpEndpoint>>().Object);
        
        var fakeHttpRequestData = new FakeHttpRequestData(fakeContext.Object);

        // act
        var result = await FundingRuleHttpEndpoint.FundingRuleTrigger(fakeHttpRequestData, null, Guid.NewGuid(), null!, fakeContext.Object);

        // assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Test, MoqAutoData]
    public async Task Then_A_New_Funding_Rules_Orchestration_Is_Scheduled(
        Guid learnerId,
        string instanceId,
        IndividualisedLearnerRecord ilr,
        Mock<FunctionContext> functionContext,
        Mock<DurableTaskClient> durableClient,
        Mock<IOptions<WorkerOptions>> workerOptions,
        Mock<ObjectSerializer> objectSerializer)
    {
        // arrange
        functionContext
            .Setup(x => x.InstanceServices.GetService(typeof(ILogger<FundingRuleHttpEndpoint>)))
            .Returns(new Mock<ILogger<FundingRuleHttpEndpoint>>().Object);

        string? capturedTaskName = null;
        LearnerData? capturedLearnerData = null;
        durableClient
            .Setup(x => x.ScheduleNewOrchestrationInstanceAsync(
                It.IsAny<TaskName>(),
                It.IsAny<object>(),
                It.IsAny<StartOrchestrationOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<TaskName, object?, StartOrchestrationOptions, CancellationToken>((taskName, data, _, _) =>
            {
                capturedTaskName = taskName.Name;
                capturedLearnerData = data as LearnerData;
            })
            .ReturnsAsync(instanceId);

        // for serializing the response
        workerOptions
            .Setup(x => x.Value)
            .Returns(new WorkerOptions { Serializer = objectSerializer.Object });
        functionContext
            .Setup(x => x.InstanceServices.GetService(typeof(IOptions<WorkerOptions>)))
            .Returns(workerOptions.Object);
        
        var fakeHttpRequestData = new FakeHttpRequestData(functionContext.Object);
        
        // act
        var result = await FundingRuleHttpEndpoint.FundingRuleTrigger(fakeHttpRequestData, ilr, learnerId, durableClient.Object, functionContext.Object);

        // assert
        result.StatusCode.Should().Be(HttpStatusCode.Accepted);
        capturedTaskName.Should().Be(nameof(FundingRuleOrchestrator.ApplyFundingRules));
        capturedLearnerData.Should().NotBeNull();
        capturedLearnerData.Id.Should().Be(learnerId);
        capturedLearnerData.Record.Should().Be(ilr);
    }
}