using Microsoft.Azure.Functions.Worker;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Activities;

public class CourseAgeCheckActivity
{
    [Function(nameof(CourseAgeCheck))]
    public async Task<RuleOutcome> CourseAgeCheck([ActivityTrigger] LearnerData learnerData, FunctionContext executionContext)
    {
        return new RuleOutcome(nameof(CourseAgeCheck), [new FundingRestriction(string.Empty, string.Empty)]);
    } 
}