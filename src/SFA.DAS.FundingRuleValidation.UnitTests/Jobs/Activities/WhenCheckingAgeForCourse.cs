using System.Text.Json;
using SFA.DAS.FundingRuleValidation.Jobs.Activities;
using SFA.DAS.FundingRuleValidation.Jobs.Activities.Models;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Activities;

public class WhenCheckingAgeForCourse
{
    [Test, MoqAutoData]
    public void Then_When_There_Are_No_Courses_Then_No_Funding_Restrictions_Are_Returned(
        RuleData ruleData,
        CourseAgeCheckParameters parameters)
    {
        parameters.MinimumAge = 25;
        parameters.MaximumAge = 26;
        ruleData.Rule.Parameters = JsonSerializer.Serialize(parameters);
        ruleData = ruleData with
        {
            Command = ruleData.Command with
            {
                Courses = []
            }
        };

        // act
        var result = CourseAgeCheckActivity.Run(ruleData, null!);
        
        // assert
        result.Should().BeEmpty();
    }

    [Test]
    [MoqInlineAutoData(19, RuleOutcome.Error)]
    [MoqInlineAutoData(20, RuleOutcome.Success)]
    [MoqInlineAutoData(25, RuleOutcome.Success)]
    [MoqInlineAutoData(30, RuleOutcome.Success)]
    [MoqInlineAutoData(31, RuleOutcome.Error)]
    public void Then_If_The_Learner_Is_The_Correct_Age_It_Passes(
        int age,
        RuleOutcome ruleOutcome,
        RuleData ruleData,
        CourseAgeCheckParameters parameters)
    {
        parameters.MinimumAge = 20;
        parameters.MaximumAge = 30;
        ruleData.Rule.CourseIds = ["Course1"];
        ruleData.Rule.Parameters = JsonSerializer.Serialize(parameters);
        ruleData = ruleData with
        {
            Command = ruleData.Command with
            {
                Courses = [new Course { Id = "Course1", AimSequenceNumber = 3, AgeAtStartOfCourse = age, }]
            }
        };
        
        // act
        var result = CourseAgeCheckActivity.Run(ruleData, null!);
        
        // assert
        result.Should().HaveCount(ruleData.Command.Courses.Count());
        result.Should().AllSatisfy(x =>
        {
            x.Outcome.Should().Be(ruleOutcome);
            ruleData.Command.Courses.Should().Contain(c => c.Id == x.CourseId && c.AimSequenceNumber == x.AimSequenceNumber);
            x.RuleId.Should().Be(ruleData.Rule.Id);
            x.RuleName.Should().Be(ruleData.Rule.IlrRuleName);
        });
    }
    
    [Test, MoqAutoData]
    public void Then_Multiple_Courses_Can_Be_Tested_Against_The_Rule(
        RuleData ruleData,
        CourseAgeCheckParameters parameters)
    {
        parameters.MinimumAge = 20;
        parameters.MaximumAge = 30;
        ruleData.Rule.CourseIds = ["Course1", "Course2"];
        ruleData.Rule.Parameters = JsonSerializer.Serialize(parameters);
        ruleData = ruleData with
        {
            Command = ruleData.Command with
            {
                Courses = [
                    new Course { Id = "Course1", AimSequenceNumber = 3, AgeAtStartOfCourse = 50, },
                    new Course { Id = "Course2", AimSequenceNumber = 4, AgeAtStartOfCourse = 50, }
                ]
            }
        };
        
        // act
        var result = CourseAgeCheckActivity.Run(ruleData, null!);
        
        // assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(x =>
        {
            x.CourseId.Should().BeOneOf("Course1", "Course2");
            x.AimSequenceNumber.Should().BeOneOf(3, 4);
            x.Outcome.Should().Be(RuleOutcome.Error);
            x.RuleId.Should().Be(ruleData.Rule.Id);
            x.RuleName.Should().Be(ruleData.Rule.IlrRuleName);
        });
    }
}