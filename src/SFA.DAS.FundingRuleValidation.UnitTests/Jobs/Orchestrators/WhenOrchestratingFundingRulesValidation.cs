using Microsoft.DurableTask;
using SFA.DAS.FundingRuleValidation.Jobs.Activities;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;
using SFA.DAS.FundingRuleValidation.Jobs.Orchestrators;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Orchestrators;

public class WhenOrchestratingFundingRulesValidation
{
    [Test, MoqAutoData]
    public async Task Then_When_There_Are_No_Rules_The_Request_Passes_Validation(Mock<TaskOrchestrationContext> context)
    {
        // arrange
        var command = new ValidateLearnerCommand(Guid.NewGuid(), 123456789, 987654321, []);
        
        context
            .Setup(x => x.GetInput<ValidateLearnerCommand>())
            .Returns(command);
        
        context
            .Setup(x => x.CallActivityAsync<List<FundingRule>>(nameof(GetActiveRulesForDateActivity.GetActiveRulesForDate), It.IsAny<DateTime>()))
            .ReturnsAsync([]);
        
        // act
        await FundingRuleOrchestrator.ApplyFundingRules(context.Object);

        // assert
        context.Verify(x => x.CallActivityAsync<RuleOutcome>(It.IsAny<string>(), It.IsAny<RuleData>()), Times.Never);
        
        context.Verify(x => x.CallActivityAsync(
                nameof(SendValidationResultActivity),
                It.Is<ValidateLearnerResult>(y => y.Status == ValidationStatus.Success && !y.RuleOutcomes.Any())
            ),
            Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_Passing_Rule_Outcomes_Are_Returned(
        List<FundingRule> rules,
        Mock<TaskOrchestrationContext> context)
    {
        // arrange
        var ruleNames = rules.Select(x => x.RuleName).ToHashSet();
        
        var command = new ValidateLearnerCommand(Guid.NewGuid(), 123456789, 987654321, []);
        
        context
            .Setup(x => x.CallActivityAsync<List<FundingRule>>(nameof(GetActiveRulesForDateActivity.GetActiveRulesForDate), It.IsAny<DateTime>()))
            .ReturnsAsync(rules);
    
        context
            .Setup(x => x.GetInput<ValidateLearnerCommand>())
            .Returns(command);
    
        foreach (var rule in rules)
        {
            context
                .Setup(x => x.CallActivityAsync<RuleOutcome>(rule.RuleName, It.IsAny<RuleData>()))
                .ReturnsAsync(new RuleOutcome(rule.RuleName, []));
        }

        ValidateLearnerResult? capturedResult = null;
        context
            .Setup(x => x.CallActivityAsync(nameof(SendValidationResultActivity), It.IsAny<object?>(), It.IsAny<TaskOptions?>()))
            .Callback<TaskName, object?, TaskOptions?>((_, x, _) => capturedResult = x as ValidateLearnerResult)
            .Returns(Task.CompletedTask);
    
        // act
        await FundingRuleOrchestrator.ApplyFundingRules(context.Object);
    
        // assert
        capturedResult.Should().NotBeNull();
        capturedResult!.CorrelationId.Should().Be(command.CorrelationId);
        capturedResult.Ukprn.Should().Be(command.Ukprn);
        capturedResult.Uln.Should().Be(command.Uln);
        capturedResult.Status.Should().Be(ValidationStatus.Success);
        capturedResult.RuleOutcomes.Should().HaveCount(3);
        capturedResult.RuleOutcomes.Should().AllSatisfy(x =>
        {
            ruleNames.Contains(x.RuleName).Should().BeTrue();
        });
    }
    
    [Test, MoqAutoData]
    public async Task Then_Failing_Rule_Outcomes_Are_Returned(
        List<FundingRule> rules,
        Mock<TaskOrchestrationContext> context)
    {
        // arrange
        var ruleNames = rules.Select(x => x.RuleName).ToHashSet();
        
        var command = new ValidateLearnerCommand(Guid.NewGuid(), 123456789, 987654321, []);
        
        context
            .Setup(x => x.CallActivityAsync<List<FundingRule>>(nameof(GetActiveRulesForDateActivity.GetActiveRulesForDate), It.IsAny<DateTime>()))
            .ReturnsAsync(rules);
    
        context
            .Setup(x => x.GetInput<ValidateLearnerCommand>())
            .Returns(command);
    
        foreach (var rule in rules)
        {
            context
                .Setup(x => x.CallActivityAsync<RuleOutcome>(rule.RuleName, It.IsAny<RuleData>()))
                .ReturnsAsync(new RuleOutcome(rule.RuleName, [new FundingRestriction("CourseId", "RestrictionName", "RestrictionType")]));
        }

        ValidateLearnerResult? capturedResult = null;
        context
            .Setup(x => x.CallActivityAsync(nameof(SendValidationResultActivity), It.IsAny<object?>(), It.IsAny<TaskOptions?>()))
            .Callback<TaskName, object?, TaskOptions?>((_, x, _) => capturedResult = x as ValidateLearnerResult)
            .Returns(Task.CompletedTask);
    
        // act
        await FundingRuleOrchestrator.ApplyFundingRules(context.Object);
    
        // assert
        capturedResult.Should().NotBeNull();
        capturedResult!.CorrelationId.Should().Be(command.CorrelationId);
        capturedResult.Ukprn.Should().Be(command.Ukprn);
        capturedResult.Uln.Should().Be(command.Uln);
        capturedResult.Status.Should().Be(ValidationStatus.Error);
        capturedResult.RuleOutcomes.Should().HaveCount(3);
        capturedResult.RuleOutcomes.Should().AllSatisfy(x =>
        {
            ruleNames.Contains(x.RuleName).Should().BeTrue();
        });
        
        var fundingRestrictions = capturedResult.RuleOutcomes.SelectMany(x => x.FundingRestrictions);
        fundingRestrictions.Should().HaveCount(3);
    }
}