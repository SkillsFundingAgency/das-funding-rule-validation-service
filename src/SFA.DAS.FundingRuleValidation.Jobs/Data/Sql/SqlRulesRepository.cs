using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.Sql;

public class SqlRulesRepository(IFundingRulesDataContext dbContext): IRulesRepository
{
    public Task<List<FundingRule>> GetActiveRulesForDatesAsync(List<DateTime> dates, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}