using Microsoft.DurableTask;
using SFA.DAS.FundingRuleValidation.Jobs.Activities;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;
using SFA.DAS.FundingRuleValidation.Jobs.Orchestrators;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Orchestrators;

public class WhenOrchestratingFundingRulesValidation
{
    [Test, MoqAutoData]
    public async Task Then_No_Rules_Returns_No_Rule_Outcomes(Mock<TaskOrchestrationContext> context)
    {
        // arrange
        context
            .Setup(x => x.CallActivityAsync<List<FundingRule>>(nameof(GetActiveRulesForDateActivity.GetActiveRulesForDate), It.IsAny<DateTime>()))
            .ReturnsAsync([]);

        // act
        var result = await FundingRuleOrchestrator.ApplyFundingRules(context.Object);

        // assert
        result.Should().BeEmpty();
    }
    
    [Test, MoqAutoData]
    public async Task Then_Rule_Outcomes_For_Each_Rule_Are_Returned(
        List<FundingRule> rules,
        Mock<TaskOrchestrationContext> context)
    {
        // arrange
        context
            .Setup(x => x.CallActivityAsync<List<FundingRule>>(nameof(GetActiveRulesForDateActivity.GetActiveRulesForDate), It.IsAny<DateTime>()))
            .ReturnsAsync(rules);

        context
            .Setup(x => x.GetInput<LearnerData>())
            .Returns(new LearnerData(Guid.NewGuid(), new IndividualisedLearnerRecord()));

        foreach (var rule in rules)
        {
            context
                .Setup(x => x.CallActivityAsync<RuleOutcome>(rule.RuleName, It.IsAny<RuleData>()))
                .ReturnsAsync(new RuleOutcome(rule.RuleName, rule.RuleName));
        }

        // act
        var result = await FundingRuleOrchestrator.ApplyFundingRules(context.Object);

        // assert
        result.Should().HaveCount(rules.Count);
        result.Should().AllSatisfy(x =>
        {
            rules.Select(r => r.RuleName).Should().Contain(x.RestrictionName);
        });
    }
}