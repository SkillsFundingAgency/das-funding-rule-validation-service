namespace SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

public static class DomainMappers
{
    extension(FundingRuleTableEntity entity)
    {
        public Domain.FundingRule ToDomain()
        {
            return new Domain.FundingRule
            {
                Id = Guid.Parse(entity.RowKey),
                RuleName = entity.RuleName
            };
        }
    }
    
    extension(IEnumerable<FundingRuleTableEntity> entities)
    {
        public IEnumerable<Domain.FundingRule> ToDomain()
        {
            return entities.Select(x => x.ToDomain());
        }
    }
}