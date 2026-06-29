using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.FundingRuleValidation.Jobs.Activities.Config;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Activities;

public class CourseAgeCheckActivity
{
    [Function(nameof(CourseAgeCheck))]
    public RuleOutcome CourseAgeCheck([ActivityTrigger] RuleData ruleData, FunctionContext executionContext)
    {
        var parameters = JsonSerializer.Deserialize<CourseAgeCheckParameters>(ruleData.Rule.Parameters)!;
        var fundingRestrictions = ruleData.Command.Courses
            .Where(x => parameters.MinimumAge > x.AgeAtStartOfCourse || x.AgeAtStartOfCourse > parameters.MaximumAge)
            .Select(x => new FundingRestriction(ruleData.Rule.Id, ruleData.Rule.RuleName, x.Id, nameof(Course.AgeAtStartOfCourse), x.AgeAtStartOfCourse.ToString()))
            .ToList();
        
        return new RuleOutcome(nameof(CourseAgeCheck), fundingRestrictions);
    } 
}