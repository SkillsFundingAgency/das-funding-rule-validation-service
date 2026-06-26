using Azure.Data.Tables;
using SFA.DAS.FundingRuleValidation.Jobs.Core;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

public class TableStorageRulesRepository(TableServiceClient tableServiceClient): IRulesRepository
{
    public async Task<List<FundingRule>> GetActiveRulesForDatesAsync(List<DateTime> dates, CancellationToken cancellationToken = default)
    {
        var rules = await FetchRulesAsync(dates, cancellationToken);
        return await PopulateCoursesAsync(rules, cancellationToken);
    }

    private async Task<List<FundingRuleTableEntity>> FetchRulesAsync(List<DateTime> dates, CancellationToken cancellationToken)
    {
        var ruleClient = tableServiceClient.GetTableClient(GlobalConstants.FundingRulesTableName);

        var filters = dates.Select(date => $"(EffectiveFrom le '{date:o}' and EffectiveTo ge '{date:o}')");
        var filter = string.Join(" or ", filters);

        var rulesPages = ruleClient.QueryAsync<FundingRuleTableEntity>(filter, cancellationToken: cancellationToken);
        var rules = new List<FundingRuleTableEntity>();
        await foreach (var page in rulesPages.AsPages().WithCancellation(cancellationToken))
        {
            rules.AddRange(page.Values);
        }

        return rules;
    }

    private async Task<List<Domain.FundingRule>> PopulateCoursesAsync(List<FundingRuleTableEntity> rules, CancellationToken cancellationToken)
    {
        var results = new List<Domain.FundingRule>();
        var courseAssociationsClient = tableServiceClient.GetTableClient(GlobalConstants.FundingRuleCourseAssociationsTableName);
        foreach (var rule in rules)
        {
            var filter = $"PartitionKey eq '{rule.RowKey}'";
            var courseAssociationsPages = courseAssociationsClient.QueryAsync<FundingRuleCourseAssociationTableEntity>(filter, cancellationToken: cancellationToken);
            var courses = new List<FundingRuleCourseAssociationTableEntity>();
            await foreach (var page in courseAssociationsPages.AsPages().WithCancellation(cancellationToken))
            {
                courses.AddRange(page.Values);
            }
            
            results.Add(rule.ToDomain(courses));
        }

        return results;
    }
}