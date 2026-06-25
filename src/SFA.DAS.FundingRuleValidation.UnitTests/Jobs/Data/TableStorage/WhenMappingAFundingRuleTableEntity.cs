using SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Data.TableStorage;

public class WhenMappingAFundingRuleTableEntity
{
    [Test, MoqAutoData]
    public void Then_The_Funding_Rule_Is_Mapped_To_The_Domain_Correctly(FundingRuleTableEntity entity, List<FundingRuleCourseAssociationTableEntity> associations)
    {
        // arrange
        entity.RowKey = Guid.NewGuid().ToString();

        // act
        var result = entity.ToDomain(associations);

        // assert
        result.Should().BeEquivalentTo(entity, o => o.ExcludingMissingMembers());
        result.CourseIds.Should().BeEquivalentTo(associations.Select(x => x.RowKey));
    }
}