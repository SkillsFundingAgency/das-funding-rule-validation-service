using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingRuleValidation.Jobs.Activities;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Orchestrators;

public class FundingRuleOrchestrator
{
    [Function(nameof(ApplyFundingRules))]
    public static async Task<List<RuleOutcome>> ApplyFundingRules([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(ApplyFundingRules));
        
        var rules = await context.CallActivityAsync<List<FundingRule>>(nameof(GetActiveRulesForDateActivity.GetActiveRulesForDate), context.CurrentUtcDateTime);
        if (rules is { Count: 0 })
        {
            logger.LogInformation("{InstanceId}: No rules found", context.InstanceId);
            return [];
        }

        var outputs = new List<RuleOutcome>();
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