using Azure.Data.Tables;
using SFA.DAS.FundingRuleValidation.Jobs.Core;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

public class TableStorageRulesRepository(TableServiceClient tableServiceClient): IRulesRepository
{
    public async Task<List<Domain.FundingRule>> GetActiveRulesForDate(DateTime date)
    {
        var client = tableServiceClient.GetTableClient(Constants.FundingRulesTableName);
        var pages = client.QueryAsync<FundingRuleTableEntity>();

        var results = new List<FundingRuleTableEntity>();
        await foreach (var page in pages.AsPages())
        {
            results.AddRange(page.Values);
        }
        
        return results.ToDomain().ToList();
    }
}