using Microsoft.Azure.Functions.Worker;
using SFA.DAS.FundingRuleValidation.Jobs.Data;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Activities;

public class GetActiveRulesForDateActivity(IRulesRepository rulesRepository)
{
    [Function(nameof(GetActiveRulesForDate))]
    public async Task<List<FundingRule>> GetActiveRulesForDate([ActivityTrigger] DateTime date, FunctionContext executionContext)
    {
        return await rulesRepository.GetActiveRulesForDate(date);
    }    
}