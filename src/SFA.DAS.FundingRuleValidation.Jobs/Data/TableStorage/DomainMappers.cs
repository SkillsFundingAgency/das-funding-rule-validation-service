namespace SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

public static class DomainMappers
{
    extension(FundingRuleTableEntity entity)
    {
        public Domain.FundingRule ToDomain(List<FundingRuleCourseAssociationTableEntity> courses)
        {
            return new Domain.FundingRule
            {
                Id = Guid.Parse(entity.RowKey),
                RuleName = entity.RuleName,
                EffectiveFrom = entity.EffectiveFrom,
                EffectiveTo = entity.EffectiveTo,
                Parameters = entity.Parameters,
                CourseIds = courses.Select(x => x.RowKey).ToList()
            };
        }
    }
}