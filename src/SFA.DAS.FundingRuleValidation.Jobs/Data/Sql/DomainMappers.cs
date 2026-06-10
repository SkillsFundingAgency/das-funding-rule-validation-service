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
                RuleName = entity.RuleName
            };
        }
    }
    
    extension(IEnumerable<FundingRuleEntity> entities)
    {
        public IEnumerable<Domain.FundingRule> ToDomain()
        {
            return entities.Select(x => x.ToDomain());
        }
    }
}