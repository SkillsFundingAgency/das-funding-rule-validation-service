using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingRuleValidation.Jobs.Core;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;
using SFA.DAS.FundingRuleValidation.Jobs.Orchestrators;

namespace SFA.DAS.FundingRuleValidation.Jobs.Endpoints;

public class FundingRuleServiceBusEndpoint
{
    [Function(nameof(FundingRuleServiceBusTrigger))]
    public static async Task FundingRuleServiceBusTrigger(
        [ServiceBusTrigger(GlobalConstants.IncomingQueueName, Connection = GlobalConstants.ServiceBusConnectionName)] ServiceBusReceivedMessage message,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ValidateLearnerCommand? command = null;
        try
        {
            command = JsonSerializer.Deserialize<ValidateLearnerCommand>(message.Body);
        }
        catch
        {
            throw new InvalidOperationException("Failed to deserialise ValidateLearnerMessage");
        }

        var logger = executionContext.GetLogger<FundingRuleServiceBusEndpoint>();
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(FundingRuleOrchestrator.ApplyFundingRules), command);
        logger.LogInformation("{InstanceId}: Started orchestration for correlated message '{CorrelationId}'", instanceId, command?.CorrelationId);
    }
}