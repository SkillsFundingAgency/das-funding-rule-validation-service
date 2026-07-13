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
        var command = new ValidateLearnerCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "123456789", "987654321", []);
        
        context
            .Setup(x => x.GetInput<ValidateLearnerCommand>())
            .Returns(command);
        
        context
            .Setup(x => x.CallActivityAsync<List<FundingRule>>(nameof(GetActiveRulesForDatesActivity), It.IsAny<List<DateTime>>()))
            .ReturnsAsync([]);
        
        // act
        await FundingRuleOrchestrator.ApplyFundingRules(context.Object);

        // assert
        context.Verify(x => x.CallActivityAsync<RuleCourseOutcome>(It.IsAny<string>(), It.IsAny<RuleData>()), Times.Never);
        
        context.Verify(x => x.CallActivityAsync(
                nameof(SendValidationResultActivity),
                It.Is<ValidateLearnerResult>(y => y.Status == ValidationStatus.Passed && !y.RuleOutcomes.Any())
            ),
            Times.Once);
    }
    
     [Test, MoqAutoData]
     public async Task Then_Passing_Rule_Outcomes_Are_Returned(
         List<FundingRule> rules,
         Course course,
         Mock<TaskOrchestrationContext> context)
     {
         // arrange
         var ruleNames = rules.Select(x => x.IlrRuleName).ToHashSet();
         course.StartDate = DateTime.UtcNow;
         var command = new ValidateLearnerCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "123456789", "987654321", [course]);
         
         context
             .Setup(x => x.CallActivityAsync<List<FundingRule>>(nameof(GetActiveRulesForDatesActivity), It.IsAny<List<DateTime>>()))
             .ReturnsAsync(rules);
     
         context
             .Setup(x => x.GetInput<ValidateLearnerCommand>())
             .Returns(command);
     
         foreach (var rule in rules)
         {
             rule.EffectiveFrom = DateTime.UtcNow.AddDays(-10);
             rule.EffectiveTo = DateTime.UtcNow.AddDays(10);
             rule.CourseIds = command.Courses.Select(x => x.Id).ToHashSet();
             context
                 .Setup(x => x.CallActivityAsync<List<RuleCourseOutcome>>(rule.RuleName, It.IsAny<RuleData>()))
                 .ReturnsAsync([new RuleCourseOutcome(rule.Id, rule.IlrRuleName, command.Courses.First().Id, command.Courses.First().AimSequenceNumber, RuleOutcome.Success, [])]);
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
         capturedResult.Status.Should().Be(ValidationStatus.Passed);
         capturedResult.RuleOutcomes.Should().HaveCount(3);
         capturedResult.RuleOutcomes.Should().AllSatisfy(x =>
         {
             ruleNames.Contains(x.RuleName).Should().BeTrue();
         });
     }
     
     [Test, MoqAutoData]
     public async Task Then_Failing_Rule_Outcomes_Are_Returned(
         List<FundingRule> rules,
         Course course,
         Mock<TaskOrchestrationContext> context)
     {
         // arrange
         var ruleNames = rules.Select(x => x.IlrRuleName).ToHashSet();
         course.StartDate = DateTime.UtcNow;
         var command = new ValidateLearnerCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "123456789", "987654321", [course]);
         
         context
             .Setup(x => x.CallActivityAsync<List<FundingRule>>(nameof(GetActiveRulesForDatesActivity), It.IsAny<List<DateTime>>()))
             .ReturnsAsync(rules);
     
         context
             .Setup(x => x.GetInput<ValidateLearnerCommand>())
             .Returns(command);
     
         foreach (var rule in rules)
         {
             rule.EffectiveFrom = DateTime.UtcNow.AddDays(-10);
             rule.EffectiveTo = DateTime.UtcNow.AddDays(10);
             rule.CourseIds = command.Courses.Select(x => x.Id).ToHashSet();
             context
                 .Setup(x => x.CallActivityAsync<List<RuleCourseOutcome>>(rule.RuleName, It.IsAny<RuleData>()))
                 .ReturnsAsync([new RuleCourseOutcome(
                     rule.Id,
                     rule.IlrRuleName,
                     command.Courses.First().Id,
                     command.Courses.First().AimSequenceNumber,
                     RuleOutcome.Error,
                     [new FundingRestriction("RestrictionName", "RestrictionType")])]);
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
         capturedResult.Status.Should().Be(ValidationStatus.Failed);
         capturedResult.RuleOutcomes.Should().HaveCount(3);
         capturedResult.RuleOutcomes.Should().AllSatisfy(x =>
         {
             ruleNames.Contains(x.RuleName).Should().BeTrue();
         });
         
         var fundingRestrictions = capturedResult.RuleOutcomes.SelectMany(x => x.FundingRestrictions);
         fundingRestrictions.Should().HaveCount(3);
     }

     [Test, MoqAutoData]
     public async Task Then_Failed_Activity_Invocations_Are_Caught_And_Do_Not_Fail_The_Evaluation_Process(
         List<FundingRule> rules,
         Course course,
         Mock<TaskOrchestrationContext> context)
     {
         // arrange
         var ruleNames = rules.Select(x => x.IlrRuleName).ToHashSet();
         course.StartDate = DateTime.UtcNow;
         var command = new ValidateLearnerCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "123456789", "987654321", [course]);
         
         context
             .Setup(x => x.CallActivityAsync<List<FundingRule>>(nameof(GetActiveRulesForDatesActivity), It.IsAny<List<DateTime>>()))
             .ReturnsAsync(rules);
     
         context
             .Setup(x => x.GetInput<ValidateLearnerCommand>())
             .Returns(command);
     
         foreach (var rule in rules)
         {
             rule.EffectiveFrom = DateTime.UtcNow.AddDays(-10);
             rule.EffectiveTo = DateTime.UtcNow.AddDays(10);
             rule.CourseIds = command.Courses.Select(x => x.Id).ToHashSet();
             context
                 .Setup(x => x.CallActivityAsync<List<RuleCourseOutcome>>(rule.RuleName, It.IsAny<RuleData>()))
                 .ThrowsAsync(new TaskFailedException(rule.RuleName, 100, new Exception("Failed to run activity")));
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
         capturedResult.Status.Should().Be(ValidationStatus.Failed);
         capturedResult.RuleOutcomes.Should().HaveCount(3);
         capturedResult.RuleOutcomes.Should().AllSatisfy(x =>
         {
             x.FundingRestrictions.Should().BeEquivalentTo([FundingRestriction.Unknown]);
             ruleNames.Contains(x.RuleName).Should().BeTrue();
         });
     }
}