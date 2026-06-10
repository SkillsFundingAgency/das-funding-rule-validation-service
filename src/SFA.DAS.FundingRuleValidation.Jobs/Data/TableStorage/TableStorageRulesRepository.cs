using Azure.Data.Tables;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

public class TableStorageRulesRepository(TableServiceClient tableServiceClient): IRulesRepository
{
    public async Task<List<Domain.FundingRule>> GetActiveRulesForDate(DateTime date)
    {
        var client = tableServiceClient.GetTableClient("FundingRules");
        var pages = client.QueryAsync<FundingRuleTableEntity>();

        var results = new List<FundingRuleTableEntity>();
        await foreach (var page in pages.AsPages())
        {
            results.AddRange(page.Values);
        }
        
        return results.ToDomain().ToList();
    }
}