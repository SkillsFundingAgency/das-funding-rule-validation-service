using Microsoft.Azure.Functions.Worker;
using SFA.DAS.FundingRuleValidation.Jobs.Activities;
using SFA.DAS.FundingRuleValidation.Jobs.Data;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Activities;

public class WhenGettingActiveRulesForDates
{
    [Test, MoqAutoData]
    public async Task Then_The_Active_Rules_Are_Returned(
        List<DateTime> dates,
        List<FundingRule> rules,
        [Frozen] Mock<IRulesRepository> rulesRepository,
        [Greedy] GetActiveRulesForDatesActivity sut)
    {
        // arrange
        List<DateTime>? capturedDates = null;
        rulesRepository
            .Setup(x => x.GetActiveRulesForDatesAsync(It.IsAny<List<DateTime>>(), It.IsAny<CancellationToken>()))
            .Callback<List<DateTime>, CancellationToken>((x, _) => capturedDates = x)
            .ReturnsAsync(rules);
        
        // act
        var result = await sut.Run(dates, Mock.Of<FunctionContext>());
        
        // assert
        result.Should().BeEquivalentTo(rules);
        capturedDates.Should().BeEquivalentTo(dates);
    }
    
    [Test, MoqAutoData]
    public async Task Then_No_Active_Rules_Are_Returned(
        List<DateTime> dates,
        [Frozen] Mock<IRulesRepository> rulesRepository,
        [Greedy] GetActiveRulesForDatesActivity sut)
    {
        // arrange
        rulesRepository
            .Setup(x => x.GetActiveRulesForDatesAsync(It.IsAny<List<DateTime>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<FundingRule>)null!);
        
        // act
        var result = await sut.Run(dates, Mock.Of<FunctionContext>());
        
        // assert
        result.Should().BeEquivalentTo(new List<FundingRule>());
    }
}