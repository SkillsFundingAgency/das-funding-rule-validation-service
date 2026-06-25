using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingRuleValidation.Jobs.Activities;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Orchestrators;

public class FundingRuleOrchestrator
{
    [Function(nameof(ApplyFundingRules))]
    public static async Task ApplyFundingRules([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(ApplyFundingRules));

        // Fetch the rules
        var rules = await context.CallActivityAsync<List<FundingRule>>(nameof(GetActiveRulesForDateActivity.GetActiveRulesForDate), context.CurrentUtcDateTime);
        if (rules is { Count: 0 })
        {
            logger.LogInformation("{InstanceId}: No rules found", context.InstanceId);
            return;
        }

        var outputs = new List<RuleOutcome>();
        var command = context.GetInput<ValidateLearnerCommand>()!;
        
        // Note: simplest thing possible, probably not how it should actually work
        foreach (var rule in rules)
        {
            logger.LogInformation("{InstanceId}: Calling {RuleName}", context.InstanceId, rule.RuleName);
            outputs.Add(await context.CallActivityAsync<RuleOutcome>(rule.RuleName, new RuleData(rule, command))); 
        }

        var status = outputs is { Count: 0 } ? ValidationStatus.Success : ValidationStatus.Error;
        var result = new ValidateLearnerResult(command.CorrelationId, command.Ukprn, command.Uln, status, outputs);
        await context.CallActivityAsync<RuleOutcome>(nameof(SendValidationResultActivity), result);
    }
}