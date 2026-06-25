using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.FundingRuleValidation.Jobs.Activities.Config;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Activities;

public class CourseAgeCheckActivity
{
    [Function(nameof(CourseAgeCheck))]
    public async Task<RuleOutcome> CourseAgeCheck([ActivityTrigger] RuleData ruleData, FunctionContext executionContext)
    {
        var fundingRestrictions = new List<FundingRestriction>();
        var parameters = JsonSerializer.Deserialize<CourseAgeCheckParameters>(ruleData.Rule.Parameters)!;
        var keys = ruleData.Rule.CourseIds.ToHashSet();
        
        foreach (var course in ruleData.Command.Courses)
        {
            if (!keys.Contains(course.Id))
            {
                // doesn't apply or passes
                continue;
            }

            if (parameters.MinimumAge <= course.AgeAtStartOfCourse
                && course.AgeAtStartOfCourse <= parameters.MaximumAge)
            {
                // passes
                continue;
            }

            fundingRestrictions.Add(new FundingRestriction(course.Id, nameof(Course.AgeAtStartOfCourse), course.AgeAtStartOfCourse.ToString()));
        }
        
        return new RuleOutcome(nameof(CourseAgeCheck), fundingRestrictions);
    } 
}