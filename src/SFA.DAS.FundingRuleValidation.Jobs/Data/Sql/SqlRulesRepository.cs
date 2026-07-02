using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.Sql;

public class SqlRulesRepository(IFundingRulesDataContext dbContext): IRulesRepository
{
    public async Task<List<FundingRule>> GetActiveRulesForDate(DateTime date)
    {
        var rules = await dbContext.FundingRules.ToListAsync();
        return rules.ToDomain().ToList();
    }
}