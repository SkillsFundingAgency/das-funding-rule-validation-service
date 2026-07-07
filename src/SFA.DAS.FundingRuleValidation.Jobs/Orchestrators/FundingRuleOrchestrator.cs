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
        
        var command = context.GetInput<ValidateLearnerCommand>()!;
        ValidateLearnerResult result;
        
        // Fetch all rules for all courses
        var courseDates = command.Courses.Select(x => x.StartDate.Date).Distinct().ToList();
        var rules = await context.CallActivityAsync<List<FundingRule>>(nameof(GetActiveRulesForDatesActivity), courseDates);
        
        if (rules is { Count: 0 })
        {
            logger.LogInformation("{InstanceId}: No rules found", context.InstanceId);
            result = new ValidateLearnerResult(command.CorrelationId, command.Ukprn, command.Uln, ValidationStatus.Success, []);
            await context.CallActivityAsync(nameof(SendValidationResultActivity), result);
            return;
        }

        var outputs = new List<RuleOutcome>();
        foreach (var rule in rules)
        {
            // get only the courses for the rule
            var courses = command.Courses.Where(x => 
                rule.CourseIds.Contains(x.Id)
                && x.StartDate >= rule.EffectiveFrom
                && x.StartDate <= rule.EffectiveTo
            ).ToList();
            
            if (courses.Count == 0)
            {
                // no courses apply to this rule
                continue;
            }
            
            // send only the applicable data
            var ruleCommand = command with { Courses = courses };
            
            logger.LogInformation("{InstanceId}: Calling {RuleName}", context.InstanceId, rule.RuleName);
            try
            {
                outputs.Add(await context.CallActivityAsync<RuleOutcome>(rule.RuleName, new RuleData(rule, ruleCommand)));
            }
            catch (TaskFailedException ex)
            {
                logger.LogError(ex, "{InstanceId}: Error calling {RuleName}, make sure the rule name is a valid Activity", context.InstanceId, rule.RuleName);
                outputs.Add(new RuleOutcome(rule.Id, rule.IlrRuleName, []));
            }
        }

        var status = outputs.SelectMany(x => x.FundingRestrictions).Any()
            ? ValidationStatus.Error
            : ValidationStatus.Success;
        result = new ValidateLearnerResult(command.CorrelationId, command.Ukprn, command.Uln, status, outputs);
        await context.CallActivityAsync(nameof(SendValidationResultActivity), result);
    }
}