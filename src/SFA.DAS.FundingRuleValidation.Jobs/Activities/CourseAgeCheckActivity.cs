using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.FundingRuleValidation.Jobs.Activities.Models;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Activities;

public static class CourseAgeCheckActivity
{
    [Function(nameof(CourseAgeCheckActivity))]
    public static RuleOutcome Run([ActivityTrigger] RuleData ruleData, FunctionContext executionContext)
    {
        var parameters = JsonSerializer.Deserialize<CourseAgeCheckParameters>(ruleData.Rule.Parameters)!;
        var fundingRestrictions = ruleData.Command.Courses
            .Where(x => parameters.MinimumAge > x.AgeAtStartOfCourse || x.AgeAtStartOfCourse > parameters.MaximumAge)
            .Select(x => new FundingRestriction(x.Id, nameof(Course.AgeAtStartOfCourse), x.AgeAtStartOfCourse.ToString()))
            .ToList();
        
        return new RuleOutcome(ruleData.Rule.Id, nameof(CourseAgeCheckActivity), fundingRestrictions);
    } 
}