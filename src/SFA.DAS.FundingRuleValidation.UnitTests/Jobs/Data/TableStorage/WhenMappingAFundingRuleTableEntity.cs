using SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Data.TableStorage;

public class WhenMappingAFundingRuleTableEntity
{
    [Test, MoqAutoData]
    public void Then_The_Funding_Rule_Is_Mapped_To_The_Domain_Correctly(FundingRuleTableEntity entity)
    {
        // arrange
        entity.RowKey = Guid.NewGuid().ToString();

        // act
        var result = entity.ToDomain();

        // assert
        result.Should().BeEquivalentTo(entity, o => o.ExcludingMissingMembers());
    }
    
    [Test, MoqAutoData]
    public void Then_Multiple_Funding_Rules_Are_Mapped_Correctly(List<FundingRuleTableEntity> entities)
    {
        // arrange
        entities.ForEach(x => x.RowKey = Guid.NewGuid().ToString());

        // act
        var results = entities.ToDomain();

        // assert
        results.Should().BeEquivalentTo(entities, o => o.ExcludingMissingMembers());
    }
}