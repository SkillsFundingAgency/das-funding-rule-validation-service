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
        result.Should().BeEquivalentTo(new RuleOutcome(ruleData.Rule.Id, nameof(CourseAgeCheckActivity), []));
    }

    [Test]
    [MoqInlineAutoData(19, true)]
    [MoqInlineAutoData(20, false)]
    [MoqInlineAutoData(25, false)]
    [MoqInlineAutoData(30, false)]
    [MoqInlineAutoData(31, true)]
    public void Then_If_The_Learner_Is_The_Correct_Age_It_Passes(
        int age,
        bool fails,
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
                Courses = [new Course { Id = "Course1", AgeAtStartOfCourse = age, }]
            }
        };
        
        // act
        var result = CourseAgeCheckActivity.Run(ruleData, null!);
        
        // assert
        result.FundingRestrictions.Any().Should().Be(fails);
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
                    new Course { Id = "Course1", AgeAtStartOfCourse = 50, },
                    new Course { Id = "Course2", AgeAtStartOfCourse = 50, }
                ]
            }
        };
        
        // act
        var result = CourseAgeCheckActivity.Run(ruleData, null!);
        
        // assert
        result.FundingRestrictions.Should().HaveCount(2);
        result.FundingRestrictions.First().CourseId.Should().Be("Course1");
        result.FundingRestrictions.Last().CourseId.Should().Be("Course2");
    }
}