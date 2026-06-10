using Azure.Data.Tables;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

public class TableStorageRulesRepository(TableServiceClient tableServiceClient): IRulesRepository
{
    public async Task<List<SFA.DAS.FundingRuleValidation.Jobs.Domain.FundingRule>> GetActiveRulesForDate(DateTime date)
    {
        var client = tableServiceClient.GetTableClient("FundingRules");
        var pages = client.QueryAsync<FundingRule>();

        var results = new List<FundingRule>();
        await foreach (var page in pages.AsPages())
        {
            results.AddRange(page.Values);
        }
        
        return results.Select(MapToDomain).ToList();
    }

    private static SFA.DAS.FundingRuleValidation.Jobs.Domain.FundingRule MapToDomain(FundingRule rule)
    {
        return new SFA.DAS.FundingRuleValidation.Jobs.Domain.FundingRule
        {
            RuleName = rule.RuleName
        };
    }
}