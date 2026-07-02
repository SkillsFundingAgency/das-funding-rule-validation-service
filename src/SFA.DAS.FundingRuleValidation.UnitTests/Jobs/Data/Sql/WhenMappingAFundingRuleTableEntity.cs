using SFA.DAS.FundingRuleValidation.Jobs.Data.Sql;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Data.Sql;

public class WhenMappingAFundingRuleEntity
{
    [Test, MoqAutoData]
    public void Then_The_Funding_Rule_Is_Mapped_To_The_Domain_Correctly(FundingRuleEntity entity)
    {
        // act
        var result = entity.ToDomain();

        // assert
        result.Should().BeEquivalentTo(entity, o => o.ExcludingMissingMembers());
    }
    
    [Test, MoqAutoData]
    public void Then_Multiple_Funding_Rules_Are_Mapped_Correctly(List<FundingRuleEntity> entities)
    {
        // act
        var results = entities.ToDomain();

        // assert
        results.Should().BeEquivalentTo(entities, o => o.ExcludingMissingMembers());
    }
}