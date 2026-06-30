namespace SFA.DAS.FundingRuleValidation.Jobs.Data.Sql;

public static class DomainMappers
{
    extension(FundingRuleEntity entity)
    {
        public Domain.FundingRule ToDomain()
        {
            return new Domain.FundingRule
            {
                Id = entity.Id,
                RuleName = entity.RuleName,
                EffectiveFrom = entity.EffectiveFrom,
                EffectiveTo = entity.EffectiveTo,
                Parameters = entity.Parameters,
                CourseIds = entity.CourseAssociations.Select(x => x.CourseId).ToHashSet()
            };
        }
    }
    
    extension(IEnumerable<FundingRuleEntity> entities)
    {
        public List<Domain.FundingRule> ToDomain()
        {
            return entities.Select(x => x.ToDomain()).ToList();
        }
    }
}