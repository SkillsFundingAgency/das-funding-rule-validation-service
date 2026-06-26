using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.Sql;

public class SqlRulesRepository(IFundingRulesDataContext dbContext): IRulesRepository
{
    public async Task<List<FundingRule>> GetActiveRulesForDatesAsync(List<DateTime> dates, CancellationToken cancellationToken = default)
    {
        var query = dbContext
            .FundingRules
            .Include(x => x.CourseAssociations).AsQueryable();

        query = dates.Aggregate(query, (current, date) => current.Where(x => x.EffectiveFrom <= date && x.EffectiveTo >= date));

        var rules = await query.ToListAsync(cancellationToken);
        return rules.ToDomain();
    }
}