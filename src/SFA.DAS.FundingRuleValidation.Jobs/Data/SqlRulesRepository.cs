using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data;

public class SqlRulesRepository: IRulesRepository
{
    public async Task<List<FundingRule>> GetActiveRulesForDate(DateTime date)
    {
        return [];
    }
}