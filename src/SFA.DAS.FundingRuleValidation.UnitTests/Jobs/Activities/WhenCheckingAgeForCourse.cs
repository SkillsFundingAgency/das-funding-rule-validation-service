using System.Text.Json;
using SFA.DAS.FundingRuleValidation.Jobs.Activities;
using SFA.DAS.FundingRuleValidation.Jobs.Activities.Config;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.UnitTests.Jobs.Activities;

public class WhenCheckingAgeForCourse
{
    [Test, MoqAutoData]
    public async Task Then_When_There_Are_No_Courses_Then_No_Funding_Restrictions_Are_Returned(
        RuleData ruleData,
        CourseAgeCheckParameters parameters,
        [Greedy] CourseAgeCheckActivity sut)
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
        var result = await sut.CourseAgeCheck(ruleData, null!);
        
        // assert
        result.Should().BeEquivalentTo(new RuleOutcome(nameof(CourseAgeCheckActivity.CourseAgeCheck), []));
    }
    
    [Test, MoqAutoData]
    public async Task Then_Rules_That_Do_Not_Apply_To_The_Learner_Courses_Do_Not_Fail(
        RuleData ruleData,
        CourseAgeCheckParameters parameters,
        [Greedy] CourseAgeCheckActivity sut)
    {
        parameters.MinimumAge = 25;
        parameters.MaximumAge = 26;
        ruleData.Rule.CourseIds = ["Course1"];
        ruleData.Rule.Parameters = JsonSerializer.Serialize(parameters);
        ruleData = ruleData with
        {
            Command = ruleData.Command with
            {
                Courses = [new Course { Id = "Course2", AgeAtStartOfCourse = 50, }]
            }
        };
        
        // act
        var result = await sut.CourseAgeCheck(ruleData, null!);
        
        // assert
        result.Should().BeEquivalentTo(new RuleOutcome(nameof(CourseAgeCheckActivity.CourseAgeCheck), []));
    }
    
    [Test]
    [MoqInlineAutoData(19, true)]
    [MoqInlineAutoData(20, false)]
    [MoqInlineAutoData(25, false)]
    [MoqInlineAutoData(30, false)]
    [MoqInlineAutoData(31, true)]
    public async Task Then_If_The_Learner_Is_The_Correct_Age_It_Passes(
        int age,
        bool fails,
        RuleData ruleData,
        CourseAgeCheckParameters parameters,
        [Greedy] CourseAgeCheckActivity sut)
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
        var result = await sut.CourseAgeCheck(ruleData, null!);
        
        // assert
        result.FundingRestrictions.Any().Should().Be(fails);
    }
    
    [Test, MoqAutoData]
    public async Task Then_Multiple_Courses_Can_Be_Tested_Against_The_Rule(
        RuleData ruleData,
        CourseAgeCheckParameters parameters,
        [Greedy] CourseAgeCheckActivity sut)
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
        var result = await sut.CourseAgeCheck(ruleData, null!);
        
        // assert
        result.FundingRestrictions.Should().HaveCount(2);
        result.FundingRestrictions.First().CourseId.Should().Be("Course1");
        result.FundingRestrictions.Last().CourseId.Should().Be("Course2");
    }
}