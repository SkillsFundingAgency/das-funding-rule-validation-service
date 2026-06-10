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
    [Function(nameof(FundingRuleTrigger))]
    public static async Task<HttpResponseData> FundingRuleTrigger(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "fundingRules/{learnerId:guid}")] HttpRequestData req,
        [FromBody] IndividualisedLearnerRecord? ilr,
        Guid learnerId,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(FundingRuleTrigger));
        if (ilr is null)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }
        
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(FundingRuleOrchestrators.FundingRuleOrchestrator), new LearnerData(learnerId, ilr));
        logger.LogInformation("{instanceId}: Started orchestration for learner '{learnerId}'.", instanceId, learnerId);
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}