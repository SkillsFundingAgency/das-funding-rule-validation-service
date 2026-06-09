using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;

namespace SFA.DAS.FundingRuleValidation.Jobs.Endpoints;

public class FundingRuleHttpEndpoint
{
    [Function("DurableFunctionsOrchestrationCSharp1_HttpStart")]
    public static async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var instanceId = 0;
        var response = req.CreateResponse(HttpStatusCode.Accepted);
        return response;
    }
}