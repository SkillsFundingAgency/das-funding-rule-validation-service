using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingRuleValidation.Jobs.Data;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Activities;

public class GetActiveRulesForDateActivity(IRulesRepository rulesRepository, ILogger<GetActiveRulesForDateActivity> logger)
{
    [Function(nameof(GetActiveRulesForDates))]
    public async Task<List<FundingRule>> GetActiveRulesForDates([ActivityTrigger] List<DateTime> dates, FunctionContext executionContext)
    {
        var result = await rulesRepository.GetActiveRulesForDatesAsync(dates, executionContext.CancellationToken);
        
        if (result is not { Count: > 0 })
        {
            logger.LogInformation("Returned no rules for dates: {QueryDate:o}", string.Join(", ", dates));
            return [];
        }

        logger.LogInformation(
            "Returned the following rules for dates: {QueryDate:o} '{RuleNames}'",
            string.Join(", ", dates),
            string.Join(", ", result.Select(x => x.RuleName)));
        return result;
    }
}