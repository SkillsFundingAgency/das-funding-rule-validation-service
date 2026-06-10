using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data;

public interface IRulesRepository
{
    Task<List<FundingRule>> GetActiveRulesForDate(DateTime date);
}