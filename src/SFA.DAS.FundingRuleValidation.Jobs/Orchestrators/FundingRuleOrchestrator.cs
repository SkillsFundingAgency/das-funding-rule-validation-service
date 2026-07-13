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
            logger.LogInformation("No active matching rules found");
            result = new ValidateLearnerResult(command.CorrelationId, command.WaitingInstanceId, command.Ukprn, command.Uln, ValidationStatus.Passed, []);
            await context.CallActivityAsync(nameof(SendValidationResultActivity), result);
            return;
        }

        var outputs = new List<RuleCourseOutcome>();
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

            logger.LogInformation("Calling {RuleName} with courses: {Courses}", rule.RuleName, courses.Select(x => x.Id));
            try
            {
                var outcomes = await context.CallActivityAsync<List<RuleCourseOutcome>>(rule.RuleName, new RuleData(rule, ruleCommand));
                if (outcomes is { Count: > 0 })
                {
                    outputs.AddRange(outcomes);
                }
            }
            catch (TaskFailedException ex)
            {
                logger.LogError(ex, "Error calling {RuleName}, make sure the rule name is a valid Activity", rule.RuleName);
                outputs.AddRange(courses.Select(x => 
                    new RuleCourseOutcome(
                        rule.Id,
                        rule.IlrRuleName,
                        x.Id,
                        x.AimSequenceNumber,
                        RuleOutcome.Error,
                        [FundingRestriction.Unknown])
                ));
            }
        }

        var status = outputs.All(x => x.Outcome == RuleOutcome.Success)
            ? ValidationStatus.Passed
            : ValidationStatus.Failed;
        result = new ValidateLearnerResult(command.CorrelationId, command.WaitingInstanceId, command.Ukprn, command.Uln, status, outputs);
        await context.CallActivityAsync(nameof(SendValidationResultActivity), result);
        logger.LogInformation("Validation complete");
    }
}