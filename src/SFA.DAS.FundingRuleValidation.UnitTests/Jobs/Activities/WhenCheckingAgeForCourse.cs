using SFA.DAS.FundingRuleValidation.Jobs.Activities;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Activities;

public class WhenCheckingAgeForCourse
{
    [Test, MoqAutoData]
    public async Task Then_An_Empty_Rule_Outcome_Is_Returned(
        ValidateLearnerCommand command,
        [Greedy] CourseAgeCheckActivity sut)
    {
        // act
        var result = await sut.CourseAgeCheck(command, null!);
        
        // assert
        result.Should().BeEquivalentTo(new RuleOutcome(nameof(CourseAgeCheckActivity.CourseAgeCheck), [new FundingRestriction(string.Empty, string.Empty)]));
    }
}