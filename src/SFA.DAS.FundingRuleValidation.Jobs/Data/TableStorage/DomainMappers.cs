using System.Text.Json;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

public static class DomainMappers
{
    extension(FundingRuleTableEntity entity)
    {
        public Domain.FundingRule ToDomain()
        {
            var courseIds = JsonSerializer.Deserialize<List<string>>(entity.Courses) ?? [];
            return new Domain.FundingRule
            {
                Id = Guid.Parse(entity.RowKey),
                RuleName = entity.RuleName,
                IlrRuleName = entity.IlrRuleName,
                IlrRuleDescription = entity.IlrRuleDescription,
                EffectiveFrom = entity.EffectiveFrom,
                EffectiveTo = entity.EffectiveTo,
                Parameters = entity.Parameters,
                CourseIds = courseIds.ToHashSet()
            };
        }
    }
}