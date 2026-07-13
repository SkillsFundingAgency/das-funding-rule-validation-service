using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingRuleValidation.Jobs.Core;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Activities;

public class SendValidationResultActivity(ServiceBusClient serviceBusClient, ILogger<SendValidationResultActivity> logger)
{
    [Function(nameof(SendValidationResultActivity))]
    public async Task Run([ActivityTrigger] ValidateLearnerResult result, FunctionContext context)
    {
        var body = JsonSerializer.Serialize(result);
        await using var sender = serviceBusClient.CreateSender(GlobalConstants.OutgoingQueueName);
        await sender.SendMessageAsync(new ServiceBusMessage(body), context.CancellationToken);
        logger.LogInformation("Sent validation response");
    }
}