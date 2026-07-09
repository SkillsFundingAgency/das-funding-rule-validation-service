using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.FundingRuleValidation.Jobs.Activities.Models;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Activities;

public static class CourseAgeCheckActivity
{
    [Function(nameof(CourseAgeCheckActivity))]
    public static List<RuleCourseOutcome> Run([ActivityTrigger] RuleData ruleData, FunctionContext executionContext)
    {
        var parameters = JsonSerializer.Deserialize<CourseAgeCheckParameters>(ruleData.Rule.Parameters)!;
        return ruleData.Command.Courses
            .Select(x =>
            {
                if (parameters.MinimumAge > x.AgeAtStartOfCourse || x.AgeAtStartOfCourse > parameters.MaximumAge)
                {
                    return new RuleCourseOutcome(
                        ruleData.Rule.Id,
                        ruleData.Rule.IlrRuleName,
                        x.Id,
                        x.AimSequenceNumber,
                        RuleOutcome.Error,
                        [new FundingRestriction(nameof(Course.AgeAtStartOfCourse), x.AgeAtStartOfCourse.ToString())]);
                }
                
                return new RuleCourseOutcome(
                    ruleData.Rule.Id,
                    ruleData.Rule.IlrRuleName,
                    x.Id,
                    x.AimSequenceNumber,
                    RuleOutcome.Success,
                    []);
            })
            .ToList();
    } 
}