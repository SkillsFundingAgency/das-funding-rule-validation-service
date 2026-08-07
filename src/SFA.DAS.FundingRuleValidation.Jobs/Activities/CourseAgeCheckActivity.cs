using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingRuleValidation.Jobs.Activities.Models;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Activities;

public partial class CourseAgeCheckActivity(ILogger<CourseAgeCheckActivity> logger)
{
    [Function(nameof(CourseAgeCheckActivity))]
    public List<RuleCourseOutcome> Run([ActivityTrigger] RuleData ruleData, FunctionContext executionContext)
    {
        using var scope = logger.BeginScope(new Dictionary<string, string>
        {
            { "CorrelationId", ruleData.Command.CorrelationId },
            { "WaitingInstanceId", ruleData.Command.WaitingInstanceId },
        });

        var parameters = JsonSerializer.Deserialize<CourseAgeCheckParameters>(ruleData.Rule.Parameters)!;
        return ruleData.Command.Courses
            .Select(x =>
            {
                if (parameters.MinimumAge > x.AgeAtStartOfCourse || x.AgeAtStartOfCourse > parameters.MaximumAge)
                {
                    LogCourseCheckFailed(x.Id, x.AimSequenceNumber);
                    return new RuleCourseOutcome(
                        ruleData.Rule.Id,
                        ruleData.Rule.IlrRuleName,
                        ruleData.Rule.IlrRuleDescription,
                        x.Id,
                        x.AimSequenceNumber,
                        RuleOutcome.Error,
                        [new FundingRestriction(nameof(Course.AgeAtStartOfCourse), x.AgeAtStartOfCourse.ToString())]);
                }
            
                LogCourseCheckPassed(x.Id, x.AimSequenceNumber);
                return new RuleCourseOutcome(
                    ruleData.Rule.Id,
                    ruleData.Rule.IlrRuleName,
                    ruleData.Rule.IlrRuleDescription,
                    x.Id,
                    x.AimSequenceNumber,
                    RuleOutcome.Success,
                    []);
            })
            .ToList();
    }

    [LoggerMessage(LogLevel.Information, "CourseAgeCheckActivity failed for course {CourseId}-{AimSequenceNumber}")]
    partial void LogCourseCheckFailed(string courseId, int aimSequenceNumber);

    [LoggerMessage(LogLevel.Information, "CourseAgeCheckActivity passed for course {CourseId}-{AimSequenceNumber}")]
    partial void LogCourseCheckPassed(string courseId, int aimSequenceNumber);
}