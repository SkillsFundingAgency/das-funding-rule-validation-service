using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingRuleValidation.Jobs.Activities;
using SFA.DAS.FundingRuleValidation.Jobs.Core;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Orchestrators;

public static class FundingRuleOrchestrator
{
    [Function(nameof(ApplyFundingRules))]
    public static async Task ApplyFundingRules([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(ApplyFundingRules));
        var command = context.GetInput<ValidateLearnerCommand>()!;
        var status = ValidationStatus.SystemError;
        List<RuleCourseOutcome> outputs = [];
        
        try
        {
            // Fetch all rules for all courses enabled for the specified dates
            var rules = await context.CallActivityAsync<List<FundingRule>>(
                nameof(GetActiveRulesForDatesActivity),
                command.Courses.Select(x => x.StartDate.Date).Distinct().ToList(),
                GlobalConstants.TaskOptions);

            if (rules is { Count: 0 })
            {
                logger.LogInformation("No active matching rules found");
                status = ValidationStatus.Passed;
                goto Finished;
            }
            
            foreach (var rule in rules)
            {
                // get only the courses for the rule
                var courses = command.Courses.Where(CourseSelector(rule)).ToList();

                if (courses.Count == 0) continue;

                // send only the applicable data
                var ruleCommand = command with { Courses = courses };

                logger.LogInformation("Calling {RuleName} with courses: {Courses}", rule.RuleName, courses.Select(x => x.Id));
                var outcomes = await context.CallActivityAsync<List<RuleCourseOutcome>>(rule.RuleName, new RuleData(rule, ruleCommand), GlobalConstants.TaskOptions);
                if (outcomes is { Count: > 0 })
                {
                    outputs.AddRange(outcomes);
                }
            }
            
            status = outputs.All(x => x.Outcome == RuleOutcome.Success)
                ? ValidationStatus.Passed
                : ValidationStatus.Failed;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Orchestrator failed");
            outputs = [];
        }
        
        Finished:
        var result = new ValidateLearnerResult(command.CorrelationId, command.WaitingInstanceId, command.Ukprn, command.Uln, status, outputs);
        await context.CallActivityAsync(nameof(SendValidationResultActivity), result, GlobalConstants.TaskOptions);
    }

    private static Func<Course, bool> CourseSelector(FundingRule rule) => x =>
        rule.CourseIds.Contains(x.Id)
        && x.StartDate >= rule.EffectiveFrom
        && x.StartDate <= rule.EffectiveTo;
}