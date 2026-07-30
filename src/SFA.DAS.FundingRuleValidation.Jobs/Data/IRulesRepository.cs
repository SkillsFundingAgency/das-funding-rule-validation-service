using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data;

public interface IRulesRepository
{
    Task<List<FundingRule>> GetActiveRulesForDatesAsync(List<DateTime> dates, CancellationToken cancellationToken = default);
}