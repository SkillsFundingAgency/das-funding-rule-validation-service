using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingRuleValidation.Jobs.Data;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Activities;

public class GetActiveRulesForDateActivity(IRulesRepository rulesRepository, ILogger<GetActiveRulesForDateActivity> logger)
{
    [Function(nameof(GetActiveRulesForDate))]
    public async Task<List<FundingRule>> GetActiveRulesForDate([ActivityTrigger] DateTime date, FunctionContext executionContext)
    {
        var result = await rulesRepository.GetActiveRulesForDate(date);
        if (result is not { Count: > 0 })
        {
            logger.LogInformation("Returned no rules for {QueryDate:o}", date);
            return [];
        }

        var ruleNames = string.Join(", ", result.Select(x => x.RuleName));
        logger.LogInformation("Returned the following rules for {QueryDate:o} '{RuleNames}'", date, ruleNames);
        return result;
    }
}