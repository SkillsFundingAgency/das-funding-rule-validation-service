using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingRuleValidation.Jobs.Data;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Orchestrators;

public class FundingRuleOrchestrators(IRulesRepository rulesRepository)
{
    [Function(nameof(FundingRuleOrchestrator))]
    public async Task<List<RuleOutcome>> FundingRuleOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(FundingRuleOrchestrator));
        
        var outputs = new List<RuleOutcome>();
        var rules = await rulesRepository.GetAll();
        if (rules is { Count: 0 })
        {
            logger.LogInformation("{InstanceId}: No rules found", context.InstanceId);
            return outputs;
        }

        var learnerData = context.GetInput<LearnerData>()!;
        
        // Note: simplest thing possible, probably not how it should actually work
        foreach (var rule in rules)
        {
            logger.LogInformation("{InstanceId}: Calling {RuleName}", context.InstanceId, rule.RuleName);
            outputs.Add(await context.CallActivityAsync<RuleOutcome>(rule.RuleName, new RuleData(rule, learnerData))); 
        }
        
        return outputs;
    }
}