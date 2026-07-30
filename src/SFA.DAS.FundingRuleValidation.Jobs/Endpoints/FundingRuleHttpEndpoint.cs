using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;
using SFA.DAS.FundingRuleValidation.Jobs.Orchestrators;

namespace SFA.DAS.FundingRuleValidation.Jobs.Endpoints;

public class FundingRuleHttpEndpoint
{
    [Function(nameof(FundingRuleHttpTrigger))]
    public static async Task<HttpResponseData> FundingRuleHttpTrigger(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "fundingRules")] HttpRequestData req,
        [FromBody] ValidateLearnerCommand? command,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<FundingRuleHttpEndpoint>();
        if (command is null)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }
        
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(FundingRuleOrchestrator.ApplyFundingRules), command);
        logger.LogInformation("{InstanceId}: Started orchestration for correlated message '{CorrelationId}'", instanceId, command.CorrelationId);
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}