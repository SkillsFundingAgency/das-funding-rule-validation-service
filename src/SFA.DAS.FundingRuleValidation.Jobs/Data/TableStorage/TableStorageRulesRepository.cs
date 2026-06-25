using Azure.Data.Tables;
using SFA.DAS.FundingRuleValidation.Jobs.Core;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

public class TableStorageRulesRepository(TableServiceClient tableServiceClient): IRulesRepository
{
    public async Task<List<Domain.FundingRule>> GetActiveRulesForDate(DateTime date)
    {
        var rules = await FetchRules(date);
        return await PopulateCourses(rules);
    }

    private async Task<List<FundingRuleTableEntity>> FetchRules(DateTime date)
    {
        var ruleClient = tableServiceClient.GetTableClient(GlobalConstants.FundingRulesTableName);
        var filter = $"EffectiveFrom le '{date:o}' and EffectiveTo ge '{date:o}'"; // TODO: consider the boundary checking here - could do some form of nullable instead?
        var rulesPages = ruleClient.QueryAsync<FundingRuleTableEntity>(filter);
        var rules = new List<FundingRuleTableEntity>();
        await foreach (var page in rulesPages.AsPages())
        {
            rules.AddRange(page.Values);
        }

        return rules;
    }

    private async Task<List<Domain.FundingRule>> PopulateCourses(List<FundingRuleTableEntity> rules)
    {
        var results = new List<Domain.FundingRule>();
        var courseAssociationsClient = tableServiceClient.GetTableClient(GlobalConstants.FundingRuleCourseAssociationsTableName);
        foreach (var rule in rules)
        {
            var filter = $"PartitionKey eq '{rule.RowKey}'";
            var courseAssociationsPages = courseAssociationsClient.QueryAsync<FundingRuleCourseAssociationTableEntity>(filter);
            var courses = new List<FundingRuleCourseAssociationTableEntity>();
            await foreach (var page in courseAssociationsPages.AsPages())
            {
                courses.AddRange(page.Values);
            }
            
            results.Add(rule.ToDomain(courses));
        }

        return results;
    }
}