using SFA.DAS.FundingRuleValidation.Jobs.Activities;
using SFA.DAS.FundingRuleValidation.Jobs.Data;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Activities;

public class WhenGettingActiveRulesForDate
{
    [Test, MoqAutoData]
    public async Task Then_The_Active_Rules_Are_Returned(
        DateTime date,
        List<FundingRule> rules,
        [Frozen] Mock<IRulesRepository> rulesRepository,
        [Greedy] GetActiveRulesForDateActivity sut)
    {
        // arrange
        rulesRepository
            .Setup(x => x.GetActiveRulesForDate(date))
            .ReturnsAsync(rules);
        
        // act
        var result = await sut.GetActiveRulesForDate(date, null!);
        
        // assert
        result.Should().BeEquivalentTo(rules);
    }
}