using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingRuleValidation.Jobs.Data;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Activities;

public partial class GetActiveRulesForDatesActivity(IRulesRepository rulesRepository, ILogger<GetActiveRulesForDatesActivity> logger)
{
    [Function(nameof(GetActiveRulesForDatesActivity))]
    public async Task<List<FundingRule>> Run([ActivityTrigger] List<DateTime> dates, FunctionContext executionContext)
    {
        var result = await rulesRepository.GetActiveRulesForDatesAsync(dates, executionContext.CancellationToken);
        if (result is not { Count: > 0 })
        {
            LogNoRules(string.Join(", ", dates));
            return [];
        }

        LogFoundRules(string.Join(", ", dates), string.Join(", ", result.Select(x => x.RuleName)));
        return result;
    }

    [LoggerMessage(LogLevel.Debug, "Returned the following rules for dates: {QueryDate:o} '{RuleNames}'")]
    partial void LogFoundRules(string queryDate, string ruleNames);

    [LoggerMessage(LogLevel.Debug, "Returned no rules for dates: {QueryDate:o}")]
    partial void LogNoRules(string queryDate);
}